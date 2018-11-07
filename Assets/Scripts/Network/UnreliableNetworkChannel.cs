using System.Collections.Generic;
using System.Net;
using UnityEngine.Analytics;
using UnityEngine;
public class UnreliableNetworkChannel : NetworkChannel{

	
	ulong maxRecvSeq;

	public UnreliableNetworkChannel(uint id, ChanelType type, EndPoint endPoint, uint totalChannels, ulong maxSeqPossible) : base(id, type, endPoint, totalChannels, maxSeqPossible)
	{
		maxRecvSeq = maxSeqPossible-1;
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
		for (i = ret.Count - 1; i >= 0 && isBiggerThan(ret[i].seq, maxRecvSeq, maxSeqPossible); i--) {}
		if (i < ret.Count - 1) {
			maxRecvSeq = ret[ret.Count - 1].seq;
		}
		return ret.GetRange(i+1, ret.Count - (i + 1));
	}

	
	
	public override void SendPacket(Serialize serializable) {
		Debug.Log("MaxSeq:" + maxSeqPossible);
		sendQueue.Enqueue(Packet.WritePacket(id, incSeq(), serializable, totalChannels, (uint)maxSeqPossible, EndPoint, PacketType.DATA));
	}
	
	public override void EnqueRecvPacket(Packet packet) {
		recvQueue.Enqueue(packet);
	}
}