using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AuthNetworkManager : MonoBehaviour {
	public class RemoteHost {
		public EndPoint _receiving_endpoint;
		public EndPoint _sending_endpoint;
		public uint UnreliableChannel, ReliableChannel, TimedChannel;
		public int Id;
	}

	private NetworkAPI _networkAPI;
	private List<RemoteHost> hosts = new List<RemoteHost>();
	public string TestRemoteIp;
	public int TestRemotePort, TestReceiveRemotePort, LocalPort, SpinLockTime;
	public uint MaxHosts;
	public uint ChannelsPerHost;
	public ulong MaxSeqPossible;
	private int _commandsCount;
	private AuthWorld _authWorld;
	void Start() {
		_commandsCount = System.Enum.GetValues(typeof (NetworkCommand)).Length;
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHost, MaxSeqPossible);
		var receiving_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort+1);
		var sending_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		_networkAPI.AddUnreliableChannel(0, sending_endpoint, receiving_endpoint);
		_networkAPI.AddUnreliableChannel(1, sending_endpoint, receiving_endpoint);
		hosts.Add(new RemoteHost(){_receiving_endpoint = receiving_endpoint, _sending_endpoint = sending_endpoint, UnreliableChannel = 0});
		_authWorld  = GameObject.FindObjectOfType<AuthWorld>();
	}

	void Update() {
		_networkAPI.UpdateSendQueues();

		List<Packet> channelLess;
		var packets = _networkAPI.Receive(out channelLess);
		foreach(var packet in packets) {
			Debug.Log(packet.channelId);
			switch(packet.channelId) {
				case 0: {
					// Unreliable channel;
					break;
				}
				case 1: {
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
			case NetworkCommand.MOVE_COMMAND: {
				int Id = -1;
				foreach(var e in hosts) {
					if (e._receiving_endpoint.Equals(packet.endPoint)) {
						Id = e.Id;
						break;
					}
				}
				if (Id == -1) {
					Debug.Log("Could not match endpoint to id");
					break;
				}
				_authWorld.MovementCommand(Id, packet.bitReader);
				Debug.Log("Movement arrived");
				break;
			}
		}
	}

	public void SendAuthEventUnreliable(Serialize ev) {
		foreach(var host in hosts) {
			_networkAPI.Send(host.UnreliableChannel, host._sending_endpoint, ev);
		}
		_networkAPI.UpdateSendQueues();
		return;
	}

	void OnDisable()
	{
		_networkAPI.Close();
	}
}
