using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public delegate void Serialize(BitWriter writer);

public class NetworkAPI {
	private static NetworkAPI _instance;

	public static NetworkAPI GetInstance() {
		if (_instance == null) {
			_instance = new NetworkAPI();
		}
		return _instance;
	}

	UdpClient _udpClient;
	int localPort = 3000;
	private NetworkAPI(){
		_udpClient = new UdpClient(localPort);
	}
	
	private Queue<Packet> readQueue;
	private Queue<Packet> sendQueue;
	private Dictionary<EndPoint, Dictionary<uint, NetworkChannel>> channelsMap; //TODO check if two EndPoint are equals
	
	int _spinLockSleepTime;
	// uint _totalChannels;
	ulong _maxSeqPossible;
	Thread _sendThread, _recvThread;
	uint _channelsPerHost;

	public void Init(int localPort, int spinLockTime, uint channelsPerHost, ulong maxSeqPossible) {
		_udpClient = new UdpClient(localPort);
		_spinLockSleepTime = spinLockTime;
		_channelsPerHost = channelsPerHost;
		_maxSeqPossible = maxSeqPossible;
		readQueue = new Queue<Packet>();
		sendQueue = new Queue<Packet>();
		channelsMap = new Dictionary<EndPoint, Dictionary<uint, NetworkChannel>>();
		_sendThread = new Thread(new ThreadStart(SendThread));
		_recvThread = new Thread(new ThreadStart(RecvThread));
		_sendThread.Start();
		_recvThread.Start();
	}

	public void Close()
	{
		_udpClient.Close();
		_sendThread.Abort();
		_recvThread.Abort();
	}

	public bool AddUnreliableChannel(uint id, EndPoint endpoint)
	{
		return AddChannel(id, ChanelType.UNRELIABLE, endpoint, 0);
	}

	public bool AddNoTimeoutReliableChannel(uint id, EndPoint endpoint)
	{
		return AddChannel(id, ChanelType.RELIABLE, endpoint, 0);
	}

	public bool AddTimeoutReliableChannel(uint id, EndPoint endpoint, uint timeout)
	{
		return AddChannel(id, ChanelType.TIMED, endpoint, timeout);
	}

	private bool AddChannel(uint id, ChanelType type, EndPoint endpoint, uint timeout) 
	{
		if (!channelsMap.ContainsKey(endpoint))
		{
			channelsMap.Add(endpoint, new Dictionary<uint, NetworkChannel>());
		}

		Dictionary<uint, NetworkChannel> channels;
		channelsMap.TryGetValue(endpoint, out channels);

		if (!channels.ContainsKey(id))
		{
			switch (type)
			{
				case ChanelType.UNRELIABLE:
					channels.Add(id, new UnreliableNetworkChannel(id, type, endpoint, _channelsPerHost, _maxSeqPossible));
					break;
				case ChanelType.RELIABLE:
				case ChanelType.TIMED:
					channels.Add(id, new ReliableNetworkChannel(id, type, endpoint, _channelsPerHost, _maxSeqPossible, timeout));
					break;
			}
			
			return true;
		}
		return false;
	}

	public bool Send(uint channel, EndPoint enpPoint, Serialize serial) 
	{
		NetworkChannel networkChannel;
		if (!getChannel(channel, enpPoint, out networkChannel)) return false;
		networkChannel.SendPacket(serial);
		return true;
	}

	public bool getChannel(uint channel, EndPoint enpPoint, out NetworkChannel networkChannel) // TODO only public for tests
	{
		networkChannel = null;
		Dictionary<uint, NetworkChannel> channels;
		if (!channelsMap.TryGetValue(enpPoint, out channels)) return false;
		if (!channels.TryGetValue(channel, out networkChannel)) return false;
		return true;
	}

	public void UpdateSendQueues()
	{
		var packetList = new List<Packet>();
		foreach (var channels in channelsMap.Values)
		{
			foreach (var channel in channels.Values)
			{
				packetList.AddRange(channel.GetPacketsToSend());
			}
		}
	
		lock (sendQueue)
		{
			foreach (var packet in packetList)
			{
				sendQueue.Enqueue(packet);
			}
		}
	}

	public List<Packet> Receive(out List<Packet> channelLessPacketList) {
		channelLessPacketList = new List<Packet>();
		lock(readQueue) {
			while (readQueue.Count > 0) {
				var packet = readQueue.Dequeue();
				NetworkChannel channel;
				if (!getChannel(packet.channelId, packet.endPoint, out channel))
				{
					channelLessPacketList.Add(packet);
				}
				else
				{
					channel.EnqueRecvPacket(packet);
				}
			}
		}
		var packetList = new List<Packet>();
		foreach (var channels in channelsMap.Values)
		{
			foreach (var channel in channels.Values)
			{
				packetList.AddRange(channel.ReceivePackets());
			}
		}
		return packetList;
	}


	public void RecvThread()
	{
		while(true) {
			EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] buffer = new byte[1000];
			int bytes = _udpClient.Client.ReceiveFrom(buffer, 1000, SocketFlags.None, ref remoteEndPoint);
			var packet = Packet.ReadPacket(buffer, (int) _channelsPerHost, 10000, remoteEndPoint);
			Debug.Log("Recv. Pakcet");
			lock(readQueue) {
				readQueue.Enqueue(packet);
			}
		}
	}

	public void SendThread()
	{
		while(true)
		{
			Packet packet = null;
			lock (sendQueue){
				if (sendQueue.Count > 0)
				{
					packet = sendQueue.Dequeue();
				}
			}
			if(packet!=null){
				var sent = _udpClient.Client.SendTo(packet.buffer.GetBuffer(), (int) packet.buffer.Length, SocketFlags.None, packet.endPoint);
			} else {
				System.Threading.Thread.Sleep(_spinLockSleepTime);
			}
		}
	}

}