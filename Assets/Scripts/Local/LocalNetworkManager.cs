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
			Debug.Log("Packet" + Time.frameCount);
			switch(packet.channelId) {
				case 0: {
					// Unreliable channel
					var reader = new BitReader(packet.buffer);
					// Read packet header and discard info
					reader.ReadInt(0, (int) MaxSeqPossible);
					reader.ReadInt(0, (int) TotalChannels);
        	reader.ReadInt(0, Enum.GetNames(typeof(PacketType)).Length);
					// Add snapshot to local world queue
					_localWorld.NewSnapshot(reader);
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
