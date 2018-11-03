using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;

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

	private List<UnreliableNetworkChannel> channels;
	
	int _spinLockSleepTime;
	uint _totalChannels;
	ulong _maxSeqPossible;
	public void Init(int localPort, int spinLockTime, uint totalChannels, ulong maxSeqPossible) {
		_udpClient = new UdpClient(localPort);	
		_spinLockSleepTime = spinLockTime;
		_totalChannels = totalChannels;
		_maxSeqPossible = maxSeqPossible;
	}

	public int AddChannel(uint id, ChanelType type, EndPoint endpoint) 
	{
		channels.Add(new UnreliableNetworkChannel(id, type, endpoint, _totalChannels, _maxSeqPossible));
		return channels.Count - 1;
	}

	public void Send(int channel, ISerial serial) 
	{
		channels[channel].Send(serial);
	}

	public List<Packet> Receive(out List<Packet> channelLessPacketList) {
		channelLessPacketList = new List<Packet>();
		lock(readQueue) {
			while (readQueue.Count > 0) {
				var packet = readQueue.Dequeue();
				UnreliableNetworkChannel pChannel = null;
				foreach(var c in channels) {
					if (c.EndPoint == packet.endPoint && c.id == packet.channelId) {
						pChannel = c;
						break;
					}
				}
				if (pChannel != null) {
					pChannel.EnqueRecvPacket(packet);
				} else {
					channelLessPacketList.Add(packet);
				}
			}
		}
		var packetList = new List<Packet>();
		foreach(var c in channels) {
			packetList.AddRange(c.Receive());
		}
		return packetList;
	}


	public void RecvThread()
	{
		while(true) {
			EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] buffer = new byte[1000];
			int bytes = _udpClient.Client.ReceiveFrom(buffer, 1000, SocketFlags.None, ref remoteEndPoint);
			var bitReader = new BitReader(new MemoryStream(buffer));
			var packet = Packet.ReadPacket(buffer, 3, 10000, remoteEndPoint);
			lock(readQueue) {
				readQueue.Enqueue(packet);
			}
		}
	}

	public void SendThread()
	{
		while(true) {
			if (sendQueue.Count >= 0) {
				var packet = sendQueue.Dequeue();
				_udpClient.Client.SendTo(packet.buffer, packet.buffer.Length, SocketFlags.None, packet.endPoint);
			} else {
				System.Threading.Thread.Sleep(_spinLockSleepTime);
			}
		}
	}

}