﻿using System.Collections;
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
	private uint MaxPlayer;
	public Transform RemotePlayerPrefab;
	public Transform MainPlayerFab;
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
		MaxPlayer = (uint)_localWorld.MaxNumberOfPlayer;
		SendReliable(new JoinCommand().Serialize);
	}
	
	// Update is called once per frame
	void Update () {
		
			List<Packet> channelLess;
			var packets = _networkAPI.Receive(out channelLess);
			_networkAPI.UpdateSendQueues();


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
				Debug.Log("JOIN RESPONSE: " + packet.endPoint);
				JoinResponseCommand joinResponseCommand = JoinResponseCommand.Deserialize(packet.bitReader, MaxPlayer);
				uint currentId = joinResponseCommand.playerId;
			//	Transform localPlayerInstance = Instantiate(MainPlayerFab, new Vector3(currentId*3, 0, 0), Quaternion.identity).GetChild(0); // TODO initial position.
			//	LocalCharacterEntity lce = localPlayerInstance.gameObject.GetComponent<LocalCharacterEntity>();
			//	lce.Id = (int)currentId;
			//	lce.Init();
			//	LocalCharacterEntity lce = Player.GetComponent<LocalCharacterEntity>();
			//	_localWorld.RemoveReference(lce.Id);
			//	lce.Id = (int)currentId;
			//	_localWorld.AddReference((int)currentId, lce);
				
				//Player = lce.gameObject;
				break;
			case NetworkCommand.JOIN_PLAYER_COMMAND:
				Debug.Log("JOIN PLAYER: " + packet.endPoint);
				JoinPlayerCommand joinPlayerCommand = JoinPlayerCommand.Deserialize(packet.bitReader, MaxPlayer);
				//Player.GetComponent<LocalCharacterEntity>().Id = (int) joinPlayerCommand.playerId;
				currentId = joinPlayerCommand.playerId;
				Transform localPlayerInstance = Instantiate(MainPlayerFab, new Vector3(currentId*3, 0, 0), Quaternion.identity); // TODO initial position.
				LocalCharacterEntity lce = localPlayerInstance.gameObject.GetComponent<LocalCharacterEntity>();
				lce.Id = (int)currentId;
				//lce.Init();
				
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
