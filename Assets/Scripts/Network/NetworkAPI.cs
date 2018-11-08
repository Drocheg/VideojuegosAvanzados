using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public delegate void Serialize(BitWriter writer);

public class NetworkAPI {
	private static NetworkAPI _instance;
	
	private System.Random _Random;

	public static NetworkAPI GetInstance() {
		if (_instance == null) {
			_instance = new NetworkAPI();
		}
		return _instance;
	}

	UdpClient _udpClient, _udpSendingClient;
	private NetworkAPI(){}
	

	private Queue<Packet> readQueue;
	private Queue<Packet> sendQueue;
	private Dictionary<EndPoint, Dictionary<uint, NetworkChannel>> channelsMap; //TODO check if two EndPoint are equals
	private float _packetLoss;
	
	int _spinLockSleepTime;
	// uint _totalChannels;
	uint _channelsPerHost;
	ulong _maxSeqPossible;
	Thread _sendThread, _recvThread;

	public void Init(int localPort, int spinLockTime, uint channelsPerHost, ulong maxSeqPossible, float packetLoss) {
		_udpClient = new UdpClient(localPort);
		_udpSendingClient = new UdpClient(localPort+1);
		_spinLockSleepTime = spinLockTime;
		_channelsPerHost = channelsPerHost;
		_maxSeqPossible = maxSeqPossible;
		readQueue = new Queue<Packet>();
		sendQueue = new Queue<Packet>();
		_Random = new System.Random();
		_packetLoss = packetLoss;
		channelsMap = new Dictionary<EndPoint, Dictionary<uint, NetworkChannel>>();
		_sendThread = new Thread(new ThreadStart(SendThread));
		_recvThread = new Thread(new ThreadStart(RecvThread));
		_sendThread.Start();
		_recvThread.Start();
	}

	public void Close()
	{
		_udpClient.Close();
		_udpSendingClient.Close();
		_sendThread.Abort();
		_recvThread.Abort();
	}

	public bool AddUnreliableChannel(uint id, EndPoint receiving_endpoint, EndPoint sending_endpoint)
	{
		return AddChannel(id, ChanelType.UNRELIABLE, receiving_endpoint, sending_endpoint, 0);
	}

	public bool AddNoTimeoutReliableChannel(uint id, EndPoint receiving_endpoint, EndPoint sending_endpoint)
	{
		return AddChannel(id, ChanelType.RELIABLE, receiving_endpoint, sending_endpoint, 0);
	}

	public bool AddTimeoutReliableChannel(uint id, EndPoint receiving_endpoint, EndPoint sending_endpoint, float timeout)
	{
		return AddChannel(id, ChanelType.TIMED, receiving_endpoint, sending_endpoint, timeout);
	}

	private bool AddChannel(uint id, ChanelType type, EndPoint receiving_endpoint, EndPoint sending_endpoint, float timeout) 
	{
		if (!channelsMap.ContainsKey(receiving_endpoint))
		{
			channelsMap.Add(receiving_endpoint, new Dictionary<uint, NetworkChannel>());
		}
		if (!channelsMap.ContainsKey(sending_endpoint))
		{
			channelsMap.Add(sending_endpoint, new Dictionary<uint, NetworkChannel>());
		}

		Dictionary<uint, NetworkChannel> channelsReceiving;
		channelsMap.TryGetValue(receiving_endpoint, out channelsReceiving);

		Dictionary<uint, NetworkChannel> channelsSending;
		channelsMap.TryGetValue(sending_endpoint, out channelsSending);
		
		if (!channelsReceiving.ContainsKey(id) && !channelsSending.ContainsKey(id))
		{
			NetworkChannel newChannel = null;
			switch (type)
			{
				case ChanelType.UNRELIABLE:
					newChannel = new UnreliableNetworkChannel(id, type, receiving_endpoint, sending_endpoint, _channelsPerHost, _maxSeqPossible);
					break;
				case ChanelType.RELIABLE:
				case ChanelType.TIMED:
					newChannel = new ReliableNetworkChannel(id, type, receiving_endpoint, sending_endpoint, _channelsPerHost, _maxSeqPossible, timeout);
					break;
			}
			channelsSending.Add(id, newChannel);
			channelsReceiving.Add(id, newChannel);
			return true;
		}
		//if (channelsReceiving.ContainsKey(id) && channelsSending.ContainsKey(id))
		//{
		//	NetworkChannel nc;
		//	channelsReceiving.TryGetValue(id, out nc);
		//	if(nc != null) nc.clear();
		//	channelsSending.TryGetValue(id, out nc);
		//	if(nc != null) nc.clear();
		//}
		
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
		EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
		byte[] buffer = new byte[1000];
		while(true) {
			int bytes = _udpClient.Client.ReceiveFrom(buffer, 1000, SocketFlags.None, ref remoteEndPoint);
			if (bytes > 0) {
				var packet = Packet.ReadPacket(buffer, (int) _channelsPerHost, (int) _maxSeqPossible, remoteEndPoint);
				lock(readQueue) {
				Debug.Log("RECV THREAD: " + readQueue.Count);
					readQueue.Enqueue(packet);
				}
			}
		}
	}

	public void SendThread()
	{
		while(true)
		{
			Packet packet = null;
			lock (sendQueue){
			Debug.Log("SEND THREAD: " + sendQueue.Count);
				if (sendQueue.Count > 0)
				{
					packet = sendQueue.Dequeue();
				}
			}
			if(packet!=null){
				if (_Random.NextDouble() > _packetLoss)
				{
					var sent = _udpSendingClient.Client.SendTo(packet.buffer.GetBuffer(), (int) packet.buffer.Length, SocketFlags.None, packet.endPoint);
				}
			} else {
				System.Threading.Thread.Sleep(_spinLockSleepTime);
			}
		}
	}

}