using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;


public class ReliableNetworkChannel : NetworkChannel {
	

	private ulong maxReturnedSeq;
	private ulong maxACK;
	private readonly float timeout;
	private float lastTime;

	private readonly Queue<Packet> auxReceiveQueue;
	private Queue<Packet> actualSendQueue;

	public ReliableNetworkChannel(uint id, ChanelType type, EndPoint receiving_endpoint, EndPoint sending_endpoint, uint totalChannels, ulong maxSeqPossible, float timeout) : base(id, type, receiving_endpoint, sending_endpoint, totalChannels, maxSeqPossible)
	{
		this.timeout = timeout;
		auxReceiveQueue = new Queue<Packet>();
		actualSendQueue = new Queue<Packet>();
		maxACK = maxSeqPossible-1;
		maxReturnedSeq = maxSeqPossible-1;
	}

	public override List<Packet> GetPacketsToSend()
	{
		// We need to return the ACK and the Packets to sned but we need to eliminate ACK from the queue
		var newQueue = new Queue<Packet>();
		var retQueue = new Queue<Packet>();
		if (Time.realtimeSinceStartup - lastTime >= timeout)
		{
			lastTime = Time.realtimeSinceStartup;
			foreach (var packet in actualSendQueue)
			{
				if (isBiggerThan(packet.seq, maxACK, maxSeqPossible))
				{
					retQueue.Enqueue(packet);
					newQueue.Enqueue(packet);
				}					
			}
			actualSendQueue = newQueue;
		}

		foreach (var packet in sendQueue)
		{
			retQueue.Enqueue(packet);
			if (packet.packetType == PacketType.DATA)
			{
				actualSendQueue.Enqueue(packet);
			}
		}

		return new List<Packet>(retQueue);
	}

	
	public override List<Packet> ReceivePackets()
	{
		bool receiveNonACK = false;
		foreach (Packet newPacket in auxReceiveQueue)
		{
			if (newPacket.packetType == PacketType.ACK)
			{
				if (isBiggerThan(newPacket.seq, maxACK, maxSeqPossible))
				{
					maxACK = newPacket.seq;	
				}
				
			}
			else
			{
				receiveNonACK = true;
				if (isBiggerThan(newPacket.seq, maxReturnedSeq, maxSeqPossible) && !recvQueue.Contains(newPacket))  //TODO do not do it in O(N^2)?
				{
					recvQueue.Enqueue(newPacket);			
				}
			}
		}
		auxReceiveQueue.Clear();
		if(!receiveNonACK) return new List<Packet>();
		var sortedPackets = new List<Packet>(recvQueue); //TODO do not sort this everytime
		sortedPackets.Sort((a, b) => (int) (MapToModule(a.seq, maxReturnedSeq, maxSeqPossible) - MapToModule(b.seq, maxReturnedSeq, maxSeqPossible)));

		var ret = new List<Packet>();
		recvQueue.Clear();
		foreach (var packet in sortedPackets)
		{
			if (isEqualThan(packet.seq, maxReturnedSeq + 1, maxSeqPossible))
			{
				maxReturnedSeq = (maxReturnedSeq + 1) % maxSeqPossible;
				ret.Add(packet);
			}
			else if (isBiggerThan(packet.seq, maxReturnedSeq + 1, maxSeqPossible))
			{
				recvQueue.Enqueue(packet);
			}else{
				throw new Exception("Invalid State Exception");
			}
		}
		SendACK(maxReturnedSeq);
		return ret;
	}	
	
	public override void EnqueRecvPacket(Packet packet) {
		auxReceiveQueue.Enqueue(packet);
	}
	
	public override void SendPacket(Serialize serializable) {
		sendQueue.Enqueue(Packet.WritePacket(id, incSeq(), serializable, totalChannels, (uint)maxSeqPossible, SendingEndPoint, PacketType.DATA));
	}
	
	

}