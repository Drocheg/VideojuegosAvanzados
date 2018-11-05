using System.Collections.Generic;
using System.Net;

public class UnreliableNetworkChannel : NetworkChannel{

	
	ulong maxRecvSeq;

	public UnreliableNetworkChannel(uint id, ChanelType type, EndPoint endPoint, uint totalChannels, ulong maxSeqPossible) : base(id, type, endPoint, totalChannels, maxSeqPossible)
	{
		maxRecvSeq = 0;
	}

	public override List<Packet> GetPacketsToSend()
	{
		var ret = new List<Packet>(sendQueue);
		sendQueue.Clear();
		return ret;
	}

	
	public override List<Packet> ReceivePackets() 
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
	
	public override void SendPacket(ISerial serializable) {
		sendQueue.Enqueue(Packet.WritePacket(id, maxSendSeq++, serializable, totalChannels, (uint)maxSeqPossible, EndPoint, PacketType.DATA));
	}
	
	public override void EnqueRecvPacket(Packet packet) {
		recvQueue.Enqueue(packet);
	}
}