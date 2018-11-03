using System.Collections.Generic;
using System.Net;

public class UnreliableNetworkChannel : INetworkChannel{
	
	ChanelType type;
	public uint id;
	public EndPoint EndPoint;
	Queue<Packet> sendQueue;
	Queue<Packet> recvQueue;

	ulong maxSendSeq;
	ulong maxRecvSeq;
	ulong maxSeqPossible;
	uint totalChannels;

	public UnreliableNetworkChannel(uint id, ChanelType type, EndPoint endPoint, uint totalChannels, ulong maxSeqPossible) {
		this.id = id;
		this.type = type;
		this.EndPoint = endPoint;
		this.totalChannels = totalChannels;
		this.maxSeqPossible = maxSeqPossible;
	}

	public void Send(ISerial serializable) {
		
		var packet = Packet.WritePacket(id, maxSendSeq++, serializable, 3, 10000, EndPoint);
		sendQueue.Enqueue(packet);
	}

	public void EnqueRecvPacket(Packet packet) {
		recvQueue.Enqueue(packet);
	}

	private static ulong mod(ulong x, ulong m) {
    return (x%m + m)%m;
	}
	private static ulong MapToModule(ulong a, ulong maxRecvSeq, ulong maxSeqPossible) {
		return mod((a+ maxSeqPossible / 2 - maxRecvSeq), maxSeqPossible);
	}
	public List<Packet> Receive() 
	{
		var ret = new List<Packet>(recvQueue);
		recvQueue.Clear();
		ret.Sort((a, b) => (int) (MapToModule(a.seq, maxRecvSeq, maxSeqPossible) - MapToModule(b.seq, maxRecvSeq, maxSeqPossible))); 
		int i;
		for (i = ret.Count - 1; i >= 0 && MapToModule(ret[i].seq, maxRecvSeq, maxSeqPossible) > maxSeqPossible / 2; i--) {}
		if (i < ret.Count - 1) {
			maxRecvSeq = ret[ret.Count - 1].seq;
		}
		return ret.GetRange(i+1, ret.Count - (i + 1));
	}	
}