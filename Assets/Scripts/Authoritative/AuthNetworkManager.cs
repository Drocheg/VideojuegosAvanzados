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
	public float TimeoutEvents, TimedChannelTimeout;
	public float PacketLoss;
	private int _commandsCount;
	private AuthWorld _authWorld;
	public AuthCharacterEntity AuthPlayer;


	void Start()
	{
		takenIds = new bool[MaxHosts];
		takenIds[0] = true; // TODO delete this when host stop being a player.
		_commandsCount = System.Enum.GetValues(typeof (NetworkCommand)).Length;
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHost, MaxSeqPossible, PacketLoss);

		//var sending_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort);
		//var receiving_endpoint = new IPEndPoint(IPAddress.Parse(TestRemoteIp), TestRemotePort + 1 );
		//_networkAPI.AddUnreliableChannel(0, receiving_endpoint, sending_endpoint);
		//_networkAPI.AddTimeoutReliableChannel(1, receiving_endpoint, sending_endpoint, 0.01f);
		//_networkAPI.AddUnreliableChannel(2, receiving_endpoint, sending_endpoint);

		// _networkAPI.AddUnreliableChannel(2, receiving_endpoint, sending_endpoint);
	    //hosts.Add(new RemoteHost(){Id = 1, _receiving_endpoint = receiving_endpoint, _sending_endpoint = sending_endpoint, UnreliableChannel = 0});
		AuthPlayer.Init();
		_authWorld  = GameObject.FindObjectOfType<AuthWorld>();

	}


	void Update() {
		List<Packet> channelLess;
		var packets = _networkAPI.Receive(out channelLess);

		_networkAPI.UpdateSendQueues();

		foreach(var packet in packets) {
			switch(packet.channelId) {
				case 0: {
					// Snapshot channel;
					break;
				}
				case 1: {
					// Reliable events channel
					ParseCommand(packet);
					break;
				}
				case 2: {
					// Unreliable events channel
					break;
				}
				case 3: {
					// Reliable timed channel
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

	int GetHostId(EndPoint packetOrigin) {
		int Id = -1;
		foreach(var e in hosts) {
			if (e._receiving_endpoint.Equals(packetOrigin)) {
				Id = e.Id;
				break;
			}
		}

		return Id;
	}

	private bool addHost(Packet packet)
	{
		EndPoint currentReceivingEndpoint = packet.endPoint;

		//var port packet.bitReader.ReadInt()
		IPEndPoint currentSendingEndPoint = new IPEndPoint(((IPEndPoint)currentReceivingEndpoint).Address, ((IPEndPoint) currentReceivingEndpoint).Port-1);
		_networkAPI.AddUnreliableChannel(0, currentReceivingEndpoint, currentSendingEndPoint);
		if(!_networkAPI.AddTimeoutReliableChannel(1, currentReceivingEndpoint, currentSendingEndPoint, TimeoutEvents)) return false;
		if(!_networkAPI.AddTimeoutReliableChannel(3, currentReceivingEndpoint, currentSendingEndPoint, TimedChannelTimeout)) return false;


		var currentId = 0;

		while (takenIds[currentId]) currentId++;
		RemoteHost newHost = new RemoteHost()
		{
			Id = currentId,
			_receiving_endpoint = currentReceivingEndpoint,
			_sending_endpoint = currentSendingEndPoint,
			UnreliableChannel = 0,
			ReliableChannel = 1,
			TimedChannel = 3,
		};

		Debug.Log("Adding HOST: " + packet.endPoint + "With ID: " + currentId);
		takenIds[currentId] = true;
		Transform remotePlayerInstance = Instantiate(RemotePlayerPrefab, new Vector3(currentId*3, 0, 0), Quaternion.identity); // TODO initial position.
		AuthCharacterEntity ace = remotePlayerInstance.gameObject.GetComponent<AuthCharacterEntity>();
		ace.Id = currentId;
		ace.Init();

		// Enviar respuesta al que se conecto
		SendAuthEventReliableToSingleHost(newHost, new JoinResponseCommand((uint)currentId, MaxHosts).Serialize);
		// Enviarle a los otros que se conecto uno nuevo
		SendAuthEventReliable(new JoinPlayerCommand((uint)currentId, MaxHosts).Serialize);
		// Enviarle un packete por cada host que hay de antes al nuevo
		foreach (var host in hosts)
		{
			SendAuthEventReliableToSingleHost(newHost, new JoinPlayerCommand((uint)host.Id, MaxHosts).Serialize);
		}
		SendAuthEventReliableToSingleHost(newHost, new JoinPlayerCommand(0, MaxHosts).Serialize);
		hosts.Add(newHost);
		//_networkAPI.Send(hosts[currentId].ReliableChannel, hosts[currentId]._sending_endpoint, );	TODO send ADD PLAYER COMMAND
		return true;
	}

	void ParseCommand(Packet packet) {
		var commandType = (NetworkCommand) packet.bitReader.ReadInt(0, System.Enum.GetValues(typeof(NetworkCommand)).Length);

		switch(commandType) {
			case NetworkCommand.MOVE_COMMAND: {
				Debug.Log("Movement from: " + packet.endPoint);
				var Id = GetHostId(packet.endPoint);
				if (Id == -1) {
					Debug.Log("Could not match endpoint to id");
					break;
				}
				_authWorld.MovementCommand(Id, packet.bitReader);
				break;
			}
			case NetworkCommand.SHOOT_COMMAND: {
				var Id = GetHostId(packet.endPoint);
				if (Id == -1) {
					Debug.Log("Could not match endpoint to id");
					break;
				}
				_authWorld.Shoot(Id, packet.bitReader);
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

	public void SendAuthProjectiles(Serialize ev) {
		foreach(var host in hosts) {
			_networkAPI.Send(2, host._sending_endpoint, ev);
		}
		_networkAPI.UpdateSendQueues();
	}

	public void SendTimedChannelEvent(Serialize ev) {
		foreach(var host in hosts) {
			_networkAPI.Send(host.TimedChannel, host._sending_endpoint, ev);
		}
	}

	void OnDisable()
	{
		_networkAPI.Close();
	}
}
