﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public abstract class NetworkChannel : INetworkChannel {
	
	public readonly uint id;
	public readonly uint MaxPacketsToSend;
	public readonly EndPoint ReceivingEndPoint;
	public readonly EndPoint SendingEndPoint;
	protected ChanelType type;
	protected Queue<Packet> sendQueue;
	protected Queue<Packet> recvQueue;
	
	protected ulong maxSendSeq;
	protected readonly ulong maxSeqPossible;
	protected readonly uint totalChannels;

	protected NetworkChannel(uint id, ChanelType type, EndPoint receiving_endpoint, EndPoint sending_endpoint, uint totalChannels, ulong maxSeqPossible, uint maxPacketsToSend)
	{
		this.type = type;
		this.id = id;
		MaxPacketsToSend = maxPacketsToSend;
		ReceivingEndPoint = receiving_endpoint;
		SendingEndPoint = sending_endpoint;
		this.maxSeqPossible = maxSeqPossible;
		this.totalChannels = totalChannels;
		maxSendSeq = 0;
		sendQueue = new Queue<Packet>();
		recvQueue = new Queue<Packet>();
	}

	public abstract bool SendPacket(Serialize serializable);

	public abstract void EnqueRecvPacket(Packet packet);
	
	public abstract List<Packet> ReceivePackets();

	public abstract List<Packet> GetPacketsToSend();
	
	
	protected static ulong mod(long x, long m) {
		return (ulong)((x%m + m)%m);
	}
	
	protected static ulong MapToModule(ulong a, ulong maxRecvSeq, ulong maxSeqPossible) {
		return mod((long)a + (long)maxSeqPossible / 2 - (long)maxRecvSeq, (long)maxSeqPossible);
	}
	
	protected void SendACK(ulong seq)
	{
		sendQueue.Enqueue(Packet.WriteACKPacket(id, seq, totalChannels, (uint)maxSeqPossible, SendingEndPoint));
	}

	public abstract void clear(); 
	
	protected bool isBiggerThan(ulong first, ulong second, ulong max)
	{
		return MapToModule(first, second, max) > max / 2;
	}
	
	protected bool isEqualThan(ulong first, ulong second, ulong max)
	{
		return MapToModule(first, second, max) == max / 2;
	}

	protected ulong incSeq()
	{
		var oldSeq = maxSendSeq;
		maxSendSeq = (maxSendSeq + 1) % maxSeqPossible;
		return oldSeq;
	}

	
}
