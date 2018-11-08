using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AuthNetworkManager : MonoBehaviour {
	public class RemoteHost {
		public EndPoint _receiving_endpoint;
		public EndPoint _sending_endpoint;
		public uint UnreliableChannel, ReliableChannel, TimedChannel;
		public int Id; // The Id of the entity this host relates to.
	}

	public Transform RemotePlayerPrefab;
	
	private NetworkAPI _networkAPI;
	private List<RemoteHost> hosts = new List<RemoteHost>();
	public string TestRemoteIp;
	public int TestRemotePort2, TestReceiveRemotePort, LocalPort, SpinLockTime;
	public uint MaxHosts;
	private bool[] takenIds;
	public uint ChannelsPerHost;
	public ulong MaxSeqPossible;
	public float TimeoutEvents;
	public float PacketLoss;
	private int _commandsCount;
	private AuthWorld _authWorld;
	void Start()
	{
		takenIds = new bool[MaxHosts];
		takenIds[0] = true; // TODO delete this when host stop being a player.
		_commandsCount = System.Enum.GetValues(typeof (NetworkCommand)).Length;
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHost, MaxSeqPossible, PacketLoss);
	//	var sending_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
	//	var receiving_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort + 1 );
	//	_networkAPI.AddUnreliableChannel(0, receiving_endpoint, sending_endpoint);
	//	_networkAPI.AddTimeoutReliableChannel(1, receiving_endpoint, sending_endpoint, TimeoutEvents);
	//	hosts.Add(new RemoteHost(){Id = 1, _receiving_endpoint = receiving_endpoint, _sending_endpoint = sending_endpoint, UnreliableChannel = 0});
		_authWorld  = GameObject.FindObjectOfType<AuthWorld>();
		MaxHosts = (uint)_authWorld.MaxNumberOfPlayers;
	}


	void Update() {
		List<Packet> channelLess;
		var packets = _networkAPI.Receive(out channelLess);
		
		_networkAPI.UpdateSendQueues();

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
		foreach (var packet in channelLess)
		{
			var commandType = (NetworkCommand) packet.bitReader.ReadInt(0, _commandsCount);
			if (commandType == NetworkCommand.JOIN_COMMAND)
			{
				if (hosts.Count < MaxHosts)
				{
					addHost(packet);
				}
			}
		}
	}

	private bool addHost(Packet packet)
	{
		EndPoint currentReceivingEndpoint = packet.endPoint;
		
		//var port packet.bitReader.ReadInt()
		IPEndPoint currentSendingEndPoint = new IPEndPoint(((IPEndPoint)currentReceivingEndpoint).Address, ((IPEndPoint) currentReceivingEndpoint).Port-1);
		_networkAPI.AddUnreliableChannel(0, currentReceivingEndpoint, currentSendingEndPoint);
		if(!_networkAPI.AddTimeoutReliableChannel(1, currentReceivingEndpoint, currentSendingEndPoint, TimeoutEvents)) return false;

		
		
		var currentId = 0;
		
		while (takenIds[currentId]) currentId++;
		RemoteHost newHost = new RemoteHost()
		{
			Id = currentId,
			_receiving_endpoint = currentReceivingEndpoint,
			_sending_endpoint = currentSendingEndPoint,
			UnreliableChannel = 0,
			ReliableChannel = 1
		};
		
		Debug.Log("Adding HOST: " + packet.endPoint + "With ID: " + currentId);
		takenIds[currentId] = true;
		//Transform remotePlayerInstance = Instantiate(RemotePlayerPrefab, new Vector3(currentId*3, 0, 0), Quaternion.identity); // TODO initial position.
		//AuthCharacterEntity ace = remotePlayerInstance.gameObject.GetComponent<AuthCharacterEntity>();
		//ace.Id = currentId;
		//ace.Init();
		//SendAuthEventReliableToSingleHost(newHost, new JoinResponseCommand((uint)currentId, MaxHosts).Serialize);
		//SendAuthEventReliable(new JoinPlayerCommand((uint)currentId, MaxHosts).Serialize);
		//foreach (var host in hosts)
		//{
		//	SendAuthEventReliableToSingleHost(newHost, new JoinPlayerCommand((uint)host.Id, MaxHosts).Serialize);
		//}
		hosts.Add(newHost);
		//_networkAPI.Send(hosts[currentId].ReliableChannel, hosts[currentId]._sending_endpoint, );	TODO send ADD PLAYER COMMAND
		return true;
	}
	void ParseCommand(Packet packet) {
		var commandType = (NetworkCommand) packet.bitReader.ReadInt(0, _commandsCount);

			Debug.Log(commandType);
		switch(commandType) {
			case NetworkCommand.MOVE_COMMAND: {
				Debug.Log("Movement from: " + packet.endPoint);
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
		//_networkAPI.UpdateSendQueues();
		return;
	}
	
	public void SendAuthEventReliable(Serialize ev) {
		foreach(var host in hosts) {
			_networkAPI.Send(host.ReliableChannel, host._sending_endpoint, ev);	
		}
		//_networkAPI.UpdateSendQueues();
		return;
	}
	
	public void SendAuthEventReliableToSingleHost(RemoteHost host, Serialize ev) {
		_networkAPI.Send(host.ReliableChannel, host._sending_endpoint, ev);	
		//_networkAPI.UpdateSendQueues();
		return;
	}

	void OnDisable()
	{
		_networkAPI.Close();
	}
}
