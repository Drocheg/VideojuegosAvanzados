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
	public uint MaxPlayer;
	public Transform RemotePlayerPrefab;
	private LocalWorld _localWorld;
	private EndPoint _receiving_endpoint;
	private EndPoint _sending_endpoint;
	public float TimeoutEvents;
	public float PacketLoss;
	private int _commandsCount;
	public GameObject Player;
	
	// Use this for initialization
	void Start () {
		_commandsCount = System.Enum.GetValues(typeof (NetworkCommand)).Length;
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHosts, MaxSeqPossible, PacketLoss);
		_sending_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		_receiving_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort+1);
		_networkAPI.AddUnreliableChannel(0, _receiving_endpoint, _sending_endpoint);
		_networkAPI.AddTimeoutReliableChannel(1, _receiving_endpoint, _sending_endpoint, TimeoutEvents);
		//_networkAPI.AddTimeoutReliableChannel(1, _endpoint, 0.01f);
		_localWorld = GameObject.FindObjectOfType<LocalWorld>();
		SendReliable(new JoinCommand().Serialize);
	}
	
	// Update is called once per frame
	void Update () {
		_networkAPI.UpdateSendQueues();

		List<Packet> channelLess;
		var packets = _networkAPI.Receive(out channelLess);

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
			}
		}
	}
	
	void ParseCommand(Packet packet) {
		var commandType = (NetworkCommand) packet.bitReader.ReadInt(0, _commandsCount);

		switch(commandType) {
			case NetworkCommand.JOIN_RESPONSE_COMMAND:
				JoinResponseCommand joinResponseCommand = JoinResponseCommand.Deserialize(packet.bitReader, MaxPlayer);
				Player.GetComponent<LocalCharacterEntity>().Id = (int) joinResponseCommand.playerId;
				break;
			case NetworkCommand.JOIN_PLAYER_COMMAND:
				JoinPlayerCommand joinPlayerCommand = JoinPlayerCommand.Deserialize(packet.bitReader, MaxPlayer);
				//Player.GetComponent<LocalCharacterEntity>().Id = (int) joinPlayerCommand.playerId;
				var currentId = (int) joinPlayerCommand.playerId;
				Transform remotePlayerInstance = Instantiate(RemotePlayerPrefab, new Vector3(currentId*5, 0, 0), Quaternion.identity); // TODO initial position.
				remotePlayerInstance.gameObject.GetComponent<AuthCharacterEntity>().Id = currentId;
				
				break;
		}
	}

	public void SendReliable(Serialize serial) {
		_networkAPI.Send(1, _sending_endpoint, serial);
	}

	void OnDisable() 
	{
		_networkAPI.Close();
	}
}
