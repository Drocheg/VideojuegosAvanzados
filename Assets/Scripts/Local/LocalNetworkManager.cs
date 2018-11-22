using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using Common;
using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LocalNetworkManager : MonoBehaviour {
	public int LocalPort, SpinLockTime;
	public uint ChannelsPerHosts, MaxSeqPossible;
	private NetworkAPI _networkAPI;
	public string TestRemoteIp;
	public int TestRemotePort;
	public uint MaxPlayers;
	public Transform RemotePlayerPrefab;
	public Transform MainPlayerFab;
	private LocalWorld _localWorld;
	private EndPoint _receiving_endpoint;
	private EndPoint _sending_endpoint;
	public float TimeoutEvents, TimedChannelTimout;
	public float PacketLoss;
	public float Latency;
	public uint MaxPacketsToSend;
	private int _commandsCount;
	public GameObject Player;
	public string playerName;
	public TextMeshProUGUI _disconnectMessage;

	// Use this for initialization
	void Start () {
		if(!string.IsNullOrEmpty(MenuVariables.MenuName)) playerName	= MenuVariables.MenuName;
		if(MenuVariables.MenuPort != 0) TestRemotePort = MenuVariables.MenuPort;
		if(!string.IsNullOrEmpty(MenuVariables.MenuIP)) TestRemoteIp	= MenuVariables.MenuIP;
		
		_commandsCount = System.Enum.GetValues(typeof (NetworkCommand)).Length;
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHosts, MaxSeqPossible, PacketLoss, MaxPacketsToSend, Latency);
		_sending_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		_receiving_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort+1);
		_networkAPI.AddUnreliableChannel(0, _receiving_endpoint, _sending_endpoint);

		_networkAPI.AddTimeoutReliableChannel(1, _receiving_endpoint, _sending_endpoint, TimeoutEvents);
		_networkAPI.AddUnreliableChannel(2, _receiving_endpoint, _sending_endpoint);
		_networkAPI.AddTimeoutReliableChannel(3, _receiving_endpoint, _sending_endpoint, TimedChannelTimout);
		_localWorld = GameObject.FindObjectOfType<LocalWorld>();
		SendReliable(new JoinCommand().Serialize);
		Debug.Log("MenuIP: " + MenuVariables.MenuIP);
		SendReliable(new PlayerInfoCommand(playerName, 0, MaxPlayers).Serialize);
	}

	// Update is called once per frame
	void Update () {
		List<Packet> channelLess;
		var packets = _networkAPI.Receive(out channelLess);
		updateSendQueuesOrDisconnect();

		if (Input.GetButtonDown("k"))
		{
			disconnect();
		}

		foreach (var packet in packets) {
			switch(packet.channelId) {
				case 0: {
					// Unreliable channel
					// Add snapshot to local world queue
					_localWorld.NewSnapshot(packet.bitReader);
					break;
				}
				case 1:
				{
					// Reliable channel
					ParseCommand(packet);
					break;
				}
				case 2: {
					Debug.Log("Using unreliable event channel");
					// UnreliableEnventChannel
					ParseCommand(packet);
					break;
				}
				case 3: {
					ParseCommand(packet);
					break;
				}
			}
		}
	}

	void ParseCommand(Packet packet) {
		var commandType = (NetworkCommand) packet.bitReader.ReadInt(0, _commandsCount);
		switch(commandType) {
			case NetworkCommand.SHOOT_COMMAND:
				_localWorld.BulletCollision(packet.bitReader);
				break;
			case NetworkCommand.PROJECTILE_SHOOT_COMMAND: {
				Debug.Log("Projectile arrived");
				_localWorld.NewProjectileShootCommand(packet.bitReader);
				break;
			}
			case NetworkCommand.PROJECTILE_EXPLODE_COMMAND: {
				_localWorld.ProjectileExplosion(packet.bitReader);
				break;
			}
			case NetworkCommand.JOIN_RESPONSE_COMMAND:
				JoinResponseCommand joinResponseCommand = JoinResponseCommand.Deserialize(packet.bitReader, MaxPlayers);
				uint currentId = joinResponseCommand.playerId;
				Debug.Log("JOIN RESPONSE my ID: " + currentId + "From endpoint: " + packet.endPoint);
				//Transform localPlayerInstance = Instantiate(MainPlayerFab, new Vector3(currentId*3, 0, 0), Quaternion.identity).GetChild(0); // TODO initial position.
				//LocalCharacterEntity lce = localPlayerInstance.gameObject.GetComponent<LocalCharacterEntity>();
				LocalCharacterEntity lce = Player.GetComponent<LocalCharacterEntity>();
				lce.Id = (int)currentId;
				lce.Init();
				break;
			case NetworkCommand.JOIN_PLAYER_COMMAND:
				JoinPlayerCommand joinPlayerCommand = JoinPlayerCommand.Deserialize(packet.bitReader, MaxPlayers);
				addPlayer(joinPlayerCommand.playerId);
				Debug.Log("JOIN PLAYER with ID: " + joinPlayerCommand.playerId + "From endpoint: " + packet.endPoint);
				break;
			case NetworkCommand.DISCONNECT_COMMAND:
				Debug.Log("Receive DISCONNECT COMMAND");
				DisconnectCommand disconnectCommand = DisconnectCommand.Deserialize(packet.bitReader, MaxPlayers);
				lce = Player.GetComponent<LocalCharacterEntity>();
				int playerId = lce.Id;
				if (playerId == disconnectCommand.playerId)
				{
					// Disconnect the player
					Debug.Log("YOU HAVE BEEN DISCONECTED"); //TODO load menu scene
					_networkAPI.Close();
				}
				else
				{
					// A player has been disconnected from the game
					Debug.Log("PLAYER " + disconnectCommand.playerId + " HAS DISCONECTED");
					_localWorld.RemoveEntity(disconnectCommand.playerId);
				}
				break;
			case NetworkCommand.GAME_STATE_COMMAND: {
				Debug.Log("Game state arrived");
				_localWorld.UpdateGameState(packet.bitReader);
				break;
			}
			case NetworkCommand.PLAYER_INFO_COMMAND:
			{
				PlayerInfoCommand playerInfoCommand = PlayerInfoCommand.Deserialize(packet.bitReader, MaxPlayers);
				Debug.Log("PlayerId: " + playerInfoCommand.playerId + "PlayerName: " + playerInfoCommand.Name);
				_localWorld.AddPlayerName((int)playerInfoCommand.playerId, playerInfoCommand.Name);
				break;
			}
		}
	}

	private void addPlayer(uint playerId)
	{
		if (_localWorld.GetCharacterEntity(playerId) == null)
		{
			Transform localPlayerInstance = Instantiate(RemotePlayerPrefab, new Vector3(playerId*3, 0, 0), Quaternion.identity); // TODO initial position.
			LocalCharacterEntity lce = localPlayerInstance.gameObject.GetComponent<LocalCharacterEntity>();
			lce.Id = (int)playerId;
			lce.Init();
		}
	}

	public void SendReliable(Serialize serial)
	{
		sendOrDisconnect(1, _sending_endpoint, serial);

	}

	private void sendOrDisconnect(uint channel, EndPoint endPoint, Serialize serial)
	{
		if (!_networkAPI.Send(channel, endPoint, serial))
		{
			Debug.Log("Channel does not exist or full. Disconnecting");
			disconnect();
		}
	}

	private void updateSendQueuesOrDisconnect()
	{
		if (!_networkAPI.UpdateSendQueues())
		{
			Debug.Log("Sending queue is full. Disconnecting");
			disconnect();
		}
	}

	public void SendUnreliable(Serialize serial) {
		sendOrDisconnect(2, _sending_endpoint, serial);
	}

	void OnDisable()
	{
		_networkAPI.Close();
	}

	private void disconnect()
	{
		_disconnectMessage.enabled = true;
		_networkAPI.ClearSendQueue();
		for (int i = 0; i < 10; i++)
		{
			SendUnreliable(new DisconnectCommand(0, MaxPlayers).Serialize);
		}
		_networkAPI.UpdateSendQueues();
		System.Threading.Thread.Sleep(1000);
		_networkAPI.Close();

		SceneManager.LoadScene("MainMenu");
	}
}
