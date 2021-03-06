using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

public delegate void Serialize(BitWriter writer);

public class WrapperPacket
{
	public Packet p;
	public float t;

	public WrapperPacket(Packet p, float t)
	{
		this.p = p;
		this.t = t;
	}
}

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
	private Queue<WrapperPacket> latencyQueue;
	private Dictionary<EndPoint, Dictionary<uint, NetworkChannel>> channelsMap; //TODO check if two EndPoint are equals
	private float _packetLoss;
	private float _latency;
	
	int _spinLockSleepTime;
	// uint _totalChannels;
	uint _channelsPerHost;
	ulong _maxSeqPossible;
	Thread _sendThread, _recvThread;
	uint _maxPacketsToSend;

	public void Init(int localPort, int spinLockTime, uint channelsPerHost, ulong maxSeqPossible, float packetLoss, uint maxPacketsToSend, float latency) {
		_udpClient = new UdpClient(localPort);
		_udpSendingClient = new UdpClient(localPort+1);
		_spinLockSleepTime = spinLockTime;
		_channelsPerHost = channelsPerHost;
		_maxSeqPossible = maxSeqPossible;
		readQueue = new Queue<Packet>();
		sendQueue = new Queue<Packet>();
		latencyQueue = new Queue<WrapperPacket>();
		_Random = new System.Random();
		_packetLoss = packetLoss;
		_latency = latency;
		channelsMap = new Dictionary<EndPoint, Dictionary<uint, NetworkChannel>>();
		_sendThread = new Thread(new ThreadStart(SendThread));
		_recvThread = new Thread(new ThreadStart(RecvThread));
		_sendThread.Start();
		_recvThread.Start();
		_maxPacketsToSend = maxPacketsToSend;
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
					newChannel = new UnreliableNetworkChannel(id, type, receiving_endpoint, sending_endpoint, _channelsPerHost, _maxSeqPossible, _maxPacketsToSend);
					break;
				case ChanelType.RELIABLE:
				case ChanelType.TIMED:
					newChannel = new ReliableNetworkChannel(id, type, receiving_endpoint, sending_endpoint, _channelsPerHost, _maxSeqPossible, timeout, _maxPacketsToSend);
					break;
			}
			channelsSending.Add(id, newChannel);
			channelsReceiving.Add(id, newChannel);
			return true;
		}
		
		
		return false;
	}

	public bool Send(uint channel, EndPoint endPoint, Serialize serial) 
	{
		NetworkChannel networkChannel;
		if (!getChannel(channel, endPoint, out networkChannel)) return false;
		return networkChannel.SendPacket(serial);
	}

	public bool getChannel(uint channel, EndPoint endPoint, out NetworkChannel networkChannel) // TODO only public for tests
	{
		networkChannel = null;
		Dictionary<uint, NetworkChannel> channels;
		if (!channelsMap.TryGetValue(endPoint, out channels)) return false;
		if (!channels.TryGetValue(channel, out networkChannel)) return false;
		return true;
	}

	public bool UpdateSendQueues()
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
				if (sendQueue.Count > _maxPacketsToSend)
				{
					return false;
				}
			}
		}
		return true;
	}

	public List<Packet> Receive(out List<Packet> channelLessPacketList) {
		channelLessPacketList = new List<Packet>();
		lock(readQueue) {
			while (readQueue.Count>0)
			{
				Packet p = readQueue.Dequeue();
				latencyQueue.Enqueue(new WrapperPacket(p, Time.realtimeSinceStartup));
			}
			
			while (true)
			{
				if(latencyQueue.Count <= 0) break;
				if (latencyQueue.Peek().t + _latency > Time.realtimeSinceStartup) break;
				var wrapperPacket = latencyQueue.Dequeue();
				var packet = wrapperPacket.p;
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
		while(true) {
			byte[] buffer = new byte[1000];
			int bytes = _udpClient.Client.ReceiveFrom(buffer, 1000, SocketFlags.None, ref remoteEndPoint);
			if (bytes > 0) {
				var packet = Packet.ReadPacket(buffer, (int) _channelsPerHost, (int) _maxSeqPossible, remoteEndPoint);
				lock(readQueue) {
					// Debug.Log("RECV THREAD: " + readQueue.Count);
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

	public void RemoveChannels(EndPoint endPoint)
	{
		channelsMap.Remove(endPoint);
	}
	
	public bool ClearChannel(EndPoint sendingEndpoint, uint channelId)
	{
		NetworkChannel networkChannel;
		if (!getChannel(channelId, sendingEndpoint, out networkChannel)) return false;
		networkChannel.clear();
		return true;
	}

	public void ClearChannels(EndPoint sendingEndpoint)
	{
		for (uint i = 0; i < _channelsPerHost; i++)
		{
			ClearChannel(sendingEndpoint, i);
		}
	}

	public void ClearSendQueue()
	{
		lock (sendQueue)
		{
			sendQueue.Clear();
		}
	}


}