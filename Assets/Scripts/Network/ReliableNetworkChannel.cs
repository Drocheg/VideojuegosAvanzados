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
		var newQueue = new Queue<Packet>();
		if (Time.realtimeSinceStartup - lastTime >= timeout)
		{
			lastTime = Time.realtimeSinceStartup;
			foreach (var packet in sendQueue)
			{
				if (packet.packetType == PacketType.ACK || isBiggerThan(packet.seq, maxACK, maxSeqPossible))
				{   
					newQueue.Enqueue(packet);
				}				
			}
			sendQueue = newQueue;
			return new List<Packet>(sendQueue);
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
		if(!receiveNonACK) return new List<Packet>();
		var sortedPackets = new List<Packet>(recvQueue); //TODO do not sort this everytime
		sortedPackets.Sort((a, b) => (int) (MapToModule(a.seq, maxReturnedSeq, maxSeqPossible) - MapToModule(b.seq, maxReturnedSeq, maxSeqPossible)));

		var ret = new List<Packet>();
		recvQueue.Clear();
		foreach (var packet in sortedPackets)
		{
			if (packet.seq == maxReturnedSeq + 1)
			{
				maxReturnedSeq++;
				ret.Add(packet);
			}
			else if (packet.seq > maxReturnedSeq)
			{
				recvQueue.Enqueue(packet);
			}else{
				throw new Exception();
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