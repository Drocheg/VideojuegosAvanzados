using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using UnityEngine;

public class LocalNetworkManager : MonoBehaviour {
	public int LocalPort, SpinLockTime;
	public uint TotalChannels, MaxSeqPossible;
	private NetworkAPI _networkAPI;
	public string TestRemoteIp;
	public int TestRemotePort;
	private LocalWorld _localWorld;

	// Use this for initialization
	void Start () {
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, TotalChannels, MaxSeqPossible);
		var endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		_networkAPI.AddUnreliableChannel(0, endpoint);
		_localWorld = GameObject.FindObjectOfType<LocalWorld>();
	}
	
	// Update is called once per frame
	void Update () {
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

	void OnDisable() 
	{
		_networkAPI.Close();
	}
}
