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
	private EndPoint _endpoint;
	
	// Use this for initialization
	void Start () {
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHosts, MaxSeqPossible);
		_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		_networkAPI.AddUnreliableChannel(0, _endpoint);
		_networkAPI.AddTimeoutReliableChannel(1, _endpoint, 0.01f);
		_localWorld = GameObject.FindObjectOfType<LocalWorld>();
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
			}
		}
	}

	public void SendReliable(Serialize serial) {
		_networkAPI.Send(1, _endpoint, serial);
	}

	void OnDisable() 
	{
		_networkAPI.Close();
	}
}
