using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using UnityEngine;

public class LocalNetworkManager : MonoBehaviour {
	public int LocalPort, SpinLockTime;
	public uint ChannelsPerHosts, MaxSeqPossible;
	private NetworkAPI _networkAPI;
	public string TestRemoteIp;
	public int TestRemotePort;
	private LocalWorld _localWorld;
	private LocalProjectileManager _localProjectileManager;
	private EndPoint _receiving_endpoint;
	private EndPoint _sending_endpoint;
	
	// Use this for initialization
	void Start () {
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHosts, MaxSeqPossible);
		_sending_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		_receiving_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort+1);
		_networkAPI.AddUnreliableChannel(0, _receiving_endpoint, _sending_endpoint);
		_networkAPI.AddTimeoutReliableChannel(1, _receiving_endpoint, _sending_endpoint, 0.01f);
		_networkAPI.AddUnreliableChannel(2, _receiving_endpoint, _sending_endpoint);
		// _networkAPI.AddUnreliableChannel(2, _receiving_endpoint, _sending_endpoint);
		_localWorld = GameObject.FindObjectOfType<LocalWorld>();
		_localProjectileManager = GameObject.FindObjectOfType<LocalProjectileManager>();
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
				case 1: {
					// Reliable events channel
					ParseCommand(packet);
					
					break;
				}
				case 2: {
					_localProjectileManager.NewSnapshot(packet.bitReader);
					break;
				}
			}
		}
	}

	void ParseCommand(Packet packet) {
		var commandType = (NetworkCommand) packet.bitReader.ReadInt(0, System.Enum.GetValues(typeof(NetworkCommand)).Length);

		switch(commandType) {
			case NetworkCommand.SHOOT_COMMAND: {
				_localWorld.BulletCollision(packet.bitReader);
				break;
			}
		}
	}

	public void SendReliable(Serialize serial) {
		_networkAPI.Send(1, _sending_endpoint, serial);
	}

	public void SendUnreliable(Serialize serial) {
		// _networkAPI.Send(2, _sending_endpoint, serial);
	}

	void OnDisable() 
	{
		_networkAPI.Close();
	}
}
