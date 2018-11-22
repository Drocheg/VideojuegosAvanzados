using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Common;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

public class AuthNetworkManager : MonoBehaviour
{
	public class RemoteHost
	{
		public EndPoint _receiving_endpoint;
		public EndPoint _sending_endpoint;
		public uint UnreliableSnapshotChannel, ReliableChannel, UnreliableEventChannel, TimedChannel;
		public int Id; // The Id of the entity this host relates to.
		public float lastReceiveTime = Time.realtimeSinceStartup;
		public string name;
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
	public float TimeoutEvents, TimedChannelTimeout;
	public float PacketLoss;
	public float Latency;
	public uint MaxPacketsToSend;
	public float MaxIdleTimeBeforeDisconnect;
	private int _commandsCount;
	private AuthWorld _authWorld;
	public AuthCharacterEntity AuthPlayer;
	public string playerName;

	void Start()
	{
		if (!string.IsNullOrEmpty(MenuVariables.MenuName)) playerName = MenuVariables.MenuName;
		if (MenuVariables.MenuPort != 0) LocalPort = MenuVariables.MenuPort;
		takenIds = new bool[MaxHosts];
		hosts = new RemoteHost[MaxHosts];
		_hostCount = 0;
		takenIds[0] = true; // TODO delete this when host stop being a player.
		_commandsCount = System.Enum.GetValues(typeof(NetworkCommand)).Length;
		_networkAPI = NetworkAPI.GetInstance();
		_networkAPI.Init(LocalPort, SpinLockTime, ChannelsPerHost, MaxSeqPossible, PacketLoss, MaxPacketsToSend,
			Latency);
		AuthPlayer.Init();
		_authWorld = GameObject.FindObjectOfType<AuthWorld>();
		StartCoroutine(DelayAddPlayerName());
	}

	IEnumerator DelayAddPlayerName()
	{
		yield return new WaitForSeconds(2);
		_authWorld.AddPlayerName(0, playerName);
	}

	void Update() {
		List<Packet> channelLess;
		var packets = _networkAPI.Receive(out channelLess);

		updateSendQueuesOrDisconnect();

		foreach(var packet in packets)
		{
			int hostId = GetHostId(packet.endPoint);
			if (hostId >= 0 && hosts[hostId]!=null)
			{
				hosts[hostId].lastReceiveTime = Time.realtimeSinceStartup;
			}
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
				if (_hostCount < MaxHosts)
				{
					addHost(packet);
				}
			}
		}

		foreach (var host in hosts)
		{
			if (host != null && Time.realtimeSinceStartup - host.lastReceiveTime  > MaxIdleTimeBeforeDisconnect)
			{
				Debug.Log("Host with id: " + host.Id + " was idle, disconnecting");
				disconnectHost(host.Id);
			}
		}
	}

	int GetHostId(EndPoint packetOrigin) {
		// TODO esto podria ser orden 1
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
		var command = JoinCommand.Deserialize(); // TODO add port for server responses in join packet. Handle change of ports.
		EndPoint currentReceivingEndpoint = packet.endPoint;

		//var port packet.bitReader.ReadInt()
		Debug.Log(packet.endPoint);
		IPEndPoint currentSendingEndPoint = new IPEndPoint(((IPEndPoint)currentReceivingEndpoint).Address, ((IPEndPoint) currentReceivingEndpoint).Port-1);

		int currentId = GetHostId(currentReceivingEndpoint);

		if (currentId >= 0)
		{
			Debug.Log("Host already exist:" +hosts[currentId]);
			return false;
		}
		// New Host

		// Add channels
		if (!_networkAPI.AddUnreliableChannel(0, currentReceivingEndpoint, currentSendingEndPoint) ||
			!_networkAPI.AddTimeoutReliableChannel(1, currentReceivingEndpoint, currentSendingEndPoint,
				TimeoutEvents) ||
			!_networkAPI.AddUnreliableChannel(2, currentReceivingEndpoint, currentSendingEndPoint) ||
			!_networkAPI.AddTimeoutReliableChannel(3, currentReceivingEndpoint, currentSendingEndPoint, TimedChannelTimeout))
		{
			return false;
		}
		// Get new ID
		currentId = 0;
		while (takenIds[currentId]) currentId++;
		// Create Host
		RemoteHost newHost = new RemoteHost()
		{
			Id = currentId,
			_receiving_endpoint = currentReceivingEndpoint,
			_sending_endpoint = currentSendingEndPoint,
			UnreliableSnapshotChannel = 0,
			ReliableChannel = 1,
			UnreliableEventChannel = 2,
			TimedChannel = 3
		};
		Debug.Log("Adding HOST: " + packet.endPoint + "With ID: " + currentId);
		takenIds[currentId] = true;
		Transform remotePlayerInstance = Instantiate(RemotePlayerPrefab, new Vector3(currentId*3, 0, 0), Quaternion.identity); // TODO initial position.
		AuthCharacterEntity ace = remotePlayerInstance.gameObject.GetComponent<AuthCharacterEntity>();
		ace.Id = currentId;
		ace.Init();
		_authWorld.AddPlayer((uint)ace.Id);
		// Enviarle a los otros que se conecto uno nuevo
		SendAuthEventReliable(new JoinPlayerCommand((uint)currentId, MaxHosts).Serialize);
		
		_hostCount += 1;
		hosts[currentId] = newHost;

		// Enviar respuesta al que se conecto
		SendAuthEventReliableToSingleHost(newHost, new JoinResponseCommand((uint)currentId, MaxHosts).Serialize);

		// Enviarle un packete por cada host que hay de antes al nuevo
		foreach (var host in hosts)
		{
			if (host != null && host != newHost)
			{
				SendAuthEventReliableToSingleHost(newHost, new JoinPlayerCommand((uint)host.Id, MaxHosts).Serialize);
			}
		}
		SendAuthEventReliableToSingleHost(newHost, new JoinPlayerCommand(0, MaxHosts).Serialize);
		SendAuthEventReliableToSingleHost(newHost, new PlayerInfoCommand(playerName, 0, MaxHosts).Serialize); //TODO timed
		
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
		if (removedHost != null)
		{
			_authWorld.RemovePlayer((uint) hostID);
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
	}

	private void removeChannels(RemoteHost host)
	{
		removeChannels(host._receiving_endpoint);
		removeChannels(host._sending_endpoint);
	}

	private void removeChannels(EndPoint endPoint)
	{
		_networkAPI.RemoveChannels(endPoint);
	}


	void ParseCommand(Packet packet) {
		var commandType = (NetworkCommand) packet.bitReader.ReadInt(0, System.Enum.GetValues(typeof(NetworkCommand)).Length);

		switch(commandType) {
			case NetworkCommand.MOVE_COMMAND: {
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
			case NetworkCommand.DISCONNECT_COMMAND:
			{
				Debug.Log("Receive Disconnect Command");
				disconnectHost(packet.endPoint);
				break;
			}
			case NetworkCommand.JOIN_COMMAND:
			{
				Debug.Log("Receive Join Command");
				addHost(packet);
				break;
			}
			case NetworkCommand.PROJECTILE_SHOOT_COMMAND:
			{
				var Id = GetHostId(packet.endPoint);
				if (Id == -1) {
					Debug.Log("Could not match endpoint to id");
					break;
				}
				_authWorld.NewProjectile(Id, packet.bitReader);
				break;
			}
			case NetworkCommand.PLAYER_INFO_COMMAND:
			{
				PlayerInfoCommand playerInfoCommand = PlayerInfoCommand.Deserialize(packet.bitReader, MaxHosts);
				int hostId = GetHostId(packet.endPoint);
				Debug.Log("PlayerInfoCommand: HostId: " + hostId);
				if(hostId >= 0)
				{
					hosts[hostId].name = playerInfoCommand.Name;
					_authWorld.AddPlayerName((uint) hostId, playerInfoCommand.Name);
					Debug.Log("PlayerId: " + hostId + "PlayerName: " + playerInfoCommand.Name);
					foreach (var host in hosts)
					{
						if (host != null && host.Id != hostId)
						{
							SendAuthEventReliableToSingleHost(host, new PlayerInfoCommand(host.name, (uint)host.Id, MaxHosts).Serialize); //TODO timed
						}
					}
				}
				
				
				break;
			}
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
