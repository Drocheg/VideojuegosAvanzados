using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AuthNetworkManager : MonoBehaviour {
	public class RemoteHost {
		public EndPoint _receiving_endpoint;
		public EndPoint _sending_endpoint;
		public uint UnreliableSnapshotChannel, ReliableChannel, UnreliableEventChannel;
		public int Id; // The Id of the entity this host relates to.
	}

	public Transform RemotePlayerPrefab;
	
	private NetworkAPI _networkAPI;
	private RemoteHost[] hosts;
	private int _hostCount;
	public string TestRemoteIp;
	public int TestRemotePort2, TestReceiveRemotePort, LocalPort, SpinLockTime;
	public uint MaxHosts;
	private bool[] takenIds;
	public uint ChannelsPerHost;
	public ulong MaxSeqPossible;
	public float TimeoutEvents;
	public float PacketLoss;
	public uint MaxPacketsToSend;
	private int _commandsCount;
	private AuthWorld _authWorld;
	public AuthCharacterEntity AuthPlayer;
	
	void Start()
	{
		takenIds = new bool[MaxHosts];
		hosts = new RemoteHost[MaxHosts];
		_hostCount = 0;
		takenIds[0] = true; // TODO delete this when host stop being a player.
		_commandsCount = System.Enum.GetValues(typeof (NetworkCommand)).Length;
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHost, MaxSeqPossible, PacketLoss, MaxPacketsToSend);
		
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
		
		updateSendQueuesOrDisconnect();

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
				if (_hostCount < MaxHosts)
				{
					addHost(packet);
				}
			}
		}
	}

	int GetHostId(EndPoint packetOrigin) {
		int Id = -1;
		foreach(var e in hosts) {
			if (e!=null && e._receiving_endpoint.Equals(packetOrigin)) {
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
		_networkAPI.AddTimeoutReliableChannel(1, currentReceivingEndpoint, currentSendingEndPoint, TimeoutEvents);
		if(!_networkAPI.AddUnreliableChannel(2, currentReceivingEndpoint, currentSendingEndPoint)) return false;

		
		
		var currentId = 0;
		
		while (takenIds[currentId]) currentId++;
		RemoteHost newHost = new RemoteHost()
		{
			Id = currentId,
			_receiving_endpoint = currentReceivingEndpoint,
			_sending_endpoint = currentSendingEndPoint,
			UnreliableSnapshotChannel = 0,
			ReliableChannel = 1,
			UnreliableEventChannel = 2
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
			if (host != null)
			{
				SendAuthEventReliableToSingleHost(newHost, new JoinPlayerCommand((uint)host.Id, MaxHosts).Serialize);
			}
		}
		SendAuthEventReliableToSingleHost(newHost, new JoinPlayerCommand(0, MaxHosts).Serialize);
		_hostCount += 1;
		hosts[currentId] = newHost;
		return true;
	}

	
	private void disconnectHost(EndPoint ep)
	{
		int playerId = GetHostId(ep);
		if (playerId >= 0)
		{
			disconnectHost(playerId);
		}
	}
	
	private void disconnectHost(int hostID)
	{
		takenIds[hostID] = false;
		RemoteHost removedHost = hosts[hostID];
		hosts[hostID] = null;
		SendAuthEventReliable(new DisconnectCommand((uint)hostID, MaxHosts).Serialize);
		for (int i = 0; i < 10; i++)
		{
			SendAuthEventUnreliableToSingleHost(removedHost, new DisconnectCommand((uint)hostID, MaxHosts).Serialize);
		}
		_networkAPI.UpdateSendQueues();
		removeChannels(removedHost);
		Debug.Log("PLAYER " + hostID + " HAS DISCONECTED");
		_authWorld.RemoveEntity((uint)hostID);
	}

	private void removeChannels(RemoteHost host)
	{
		removeChannels(host._receiving_endpoint);
	}
	
	private void removeChannels(EndPoint endPoint)
	{
		_networkAPI.RemoveChannels(endPoint);
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
				Debug.Log("Receive Shoot Command");
				var Id = GetHostId(packet.endPoint);
				if (Id == -1) {
					Debug.Log("Could not match endpoint to id");
					break;
				}
				_authWorld.Shoot(Id, packet.bitReader);
				break;
			}
			case NetworkCommand.DISCONNECT_COMMAND:
			{
				Debug.Log("Receive Disconnect Command");
				var Id = GetHostId(packet.endPoint);
				disconnectHost(Id);
				break;
			}
			// TODO join command when connected?
		}
	}
	
	public void SendAuthSnapshotUnreliable(Serialize ev) {
		foreach(var host in hosts) {
			if (host != null)
			{
				sendOrDisconnect(host.UnreliableSnapshotChannel, host._sending_endpoint, ev);	
			}
		}
	}

	private void SendAuthEventUnreliable(Serialize ev) {
		foreach(var host in hosts) {
			if (host != null)
			{
				sendOrDisconnect(host.UnreliableEventChannel, host._sending_endpoint, ev);
			}
		}
	}
	
	public void SendAuthEventReliable(Serialize ev) {
		foreach(var host in hosts) {
			if (host != null)
			{
				sendOrDisconnect(host.ReliableChannel, host._sending_endpoint, ev);	
			}
			
		}
	}
	
	public void SendAuthEventReliableToSingleHost(RemoteHost host, Serialize ev) {
		sendOrDisconnect(host.ReliableChannel, host._sending_endpoint, ev);	
	}
	
	public void SendAuthEventUnreliableToSingleHost(RemoteHost host, Serialize ev) {
		sendOrDisconnect(host.UnreliableEventChannel, host._sending_endpoint, ev);
	}

	public void SendAuthProjectiles(Serialize ev) {
		foreach(var host in hosts) {
			if (host != null)
			{
				sendOrDisconnect(2, host._sending_endpoint, ev);
			}
		}
	}

	private void sendOrDisconnect(uint channel, EndPoint endPoint, Serialize serial)
	{
		if (!_networkAPI.Send(channel, endPoint, serial))
		{
			Debug.Log("Channel does not exist or full. Disconnecting player." + endPoint);
			disconnectHost(endPoint);
		}
	}
	
	private void updateSendQueuesOrDisconnect()
	{
		if (!_networkAPI.UpdateSendQueues())
		{
			Debug.Log("Sending queue is too full, disconnecting all players and terminating server");
			_networkAPI.ClearSendQueue();
			foreach (var host in hosts)
			{
				if (host != null)
				{
					disconnectHost(host.Id);
				}
			}
			_networkAPI.Close();
		}
	}
	
	void OnDisable()
	{
		_networkAPI.Close();
	}
}
