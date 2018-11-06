using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AuthNetworkManager : MonoBehaviour {
	public class RemoteHost {
		public EndPoint endPoint;
		public uint UnreliableChannel, ReliableChannel, TimedChannel;
	}

	private NetworkAPI _networkAPI;
	private List<RemoteHost> hosts = new List<RemoteHost>();
	public string TestRemoteIp;
	public int TestRemotePort, LocalPort, SpinLockTime;
	public uint MaxHosts;
	public ulong MaxSeqPossible;
	void Start() {
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, MaxHosts * 3, MaxSeqPossible);
		var endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		_networkAPI.AddUnreliableChannel(0, endpoint);
		hosts.Add(new RemoteHost(){endPoint = endpoint, UnreliableChannel = 0});
	}

	public void SendAuthEventUnreliable(Serialize ev) {
		foreach(var host in hosts) {
			_networkAPI.Send(host.UnreliableChannel, host.endPoint, ev);
		}
		return;
	}

	void OnDisable()
	{
		_networkAPI.Close();
	}
}
