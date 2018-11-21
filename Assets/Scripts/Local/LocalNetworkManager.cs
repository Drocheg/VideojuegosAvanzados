using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using JetBrains.Annotations;
using UnityEngine;

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
	public float TimeoutEvents;
	public float PacketLoss;
	public uint MaxPacketsToSend;
	private int _commandsCount;
	public GameObject Player;
	
	// Use this for initialization
	void Start () {
		_commandsCount = System.Enum.GetValues(typeof (NetworkCommand)).Length;
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHosts, MaxSeqPossible, PacketLoss, MaxPacketsToSend);
		_sending_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		_receiving_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort+1);
		_networkAPI.AddUnreliableChannel(0, _receiving_endpoint, _sending_endpoint);

		_networkAPI.AddTimeoutReliableChannel(1, _receiving_endpoint, _sending_endpoint, TimeoutEvents);
		_networkAPI.AddUnreliableChannel(2, _receiving_endpoint, _sending_endpoint);
		_localWorld = GameObject.FindObjectOfType<LocalWorld>();
		SendReliable(new JoinCommand().Serialize);
	}
	
	// Update is called once per frame
	void Update () {
		List<Packet> channelLess;
		var packets = _networkAPI.Receive(out channelLess);

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
				_localWorld.NewProjectileShootCommand(packet.bitReader);
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
		
		_networkAPI.ClearSendQueue();
		for (int i = 0; i < 10; i++)
		{
			SendUnreliable(new DisconnectCommand(0, MaxPlayers).Serialize);
		}
		_networkAPI.UpdateSendQueues();
		System.Threading.Thread.Sleep(1000);
		_networkAPI.Close();
	}
}
