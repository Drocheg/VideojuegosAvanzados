using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;


public class ReliableNetworkChannel : NetworkChannel {
	

	private ulong maxReturnedSeq;
	private ulong maxACK;
	private readonly ulong timeout;
	private float lastTime;

	private readonly Queue<Packet> auxReceiveQueue;

	public ReliableNetworkChannel(uint id, ChanelType type, EndPoint endPoint, uint totalChannels, ulong maxSeqPossible, ulong timeout) : base(id, type, endPoint, totalChannels, maxSeqPossible)
	{
		this.timeout = timeout;
		auxReceiveQueue = new Queue<Packet>();
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
			foreach (var packet in sendQueue)
			{
				if (packet.packetType == PacketType.ACK)
				{   
					retQueue.Enqueue(packet);
				}
				else
				{
					if (isBiggerThan(packet.seq, maxACK, maxSeqPossible))
					{
						retQueue.Enqueue(packet);
						newQueue.Enqueue(packet);
					}
				}					
			}
			sendQueue = newQueue;
			return new List<Packet>(retQueue);
		}
		return new List<Packet>();
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
	
	public override void SendPacket(ISerial serializable) {
		sendQueue.Enqueue(Packet.WritePacket(id, incSeq(), serializable, totalChannels, (uint)maxSeqPossible, EndPoint, PacketType.DATA));
	}
	
	

}