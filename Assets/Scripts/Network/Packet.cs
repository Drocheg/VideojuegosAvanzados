
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public enum PacketType{
    ACK,
    DATA,
}

public class Packet
{
    public uint channelId;
    public EndPoint endPoint;
	public ulong seq;
    public MemoryStream buffer;
    public PacketType packetType;
    public BitReader bitReader;
    private Packet(uint channelId, ulong seq, MemoryStream buffer, EndPoint endPoint, PacketType packetType) {
        this.channelId = channelId;
        this.seq = seq;
        this.buffer = buffer;
        this.endPoint = endPoint;
        this.packetType = packetType;
    }
    private Packet(uint channelId, ulong seq, BitReader bitReader, EndPoint endPoint, PacketType packetType) {
        this.channelId = channelId;
        this.seq = seq;
        this.bitReader = bitReader;
        this.endPoint = endPoint;
        this.packetType = packetType;
    }

    public static Packet WritePacket(uint channel, ulong seq, Serialize payload, uint channels, uint maxSeq, EndPoint endPoint, PacketType packetType)
    {
        var bitWriter = new BitWriter(1000);
		bitWriter.WriteInt(seq, 0, maxSeq);
		bitWriter.WriteInt(channel, 0, channels);
        bitWriter.WriteInt((ulong)packetType, 0, (uint)Enum.GetNames(typeof(PacketType)).Length);
		payload(bitWriter);
		bitWriter.Flush();
        bitWriter.Reset();
        return new Packet(channel, seq, bitWriter.GetBuffer(), endPoint, packetType);
    }

    public static Packet WriteACKPacket(uint channel, ulong seq, uint channels, uint maxSeq,
        EndPoint endPoint)
    {
        var bitWriter = new BitWriter(1000);
        bitWriter.WriteInt(seq, 0, maxSeq);
        bitWriter.WriteInt(channel, 0, channels);
        bitWriter.WriteInt((ulong)PacketType.ACK, 0, (uint)Enum.GetNames(typeof(PacketType)).Length);
        bitWriter.Flush();
        bitWriter.Reset();
        return new Packet(channel, seq, bitWriter.GetBuffer(), endPoint, PacketType.ACK);
    }

    public static Packet ReadPacket(byte[] buffer, int channels, int maxSeq, EndPoint endPoint)
    {
        var ms = new MemoryStream(buffer);
        var reader = new BitReader(ms);
		var seq = (uint) reader.ReadInt(0, maxSeq);
        var channel = (uint) reader.ReadInt(0, channels);
        var packetType = (PacketType) reader.ReadInt(0, Enum.GetNames(typeof(PacketType)).Length);        
        return new Packet(channel, seq, reader, endPoint, packetType);
    }

    protected bool Equals(Packet other)
    {
        return channelId == other.channelId && Equals(endPoint, other.endPoint) && seq == other.seq && packetType == other.packetType;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Packet) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int) channelId;
            hashCode = (hashCode * 397) ^ (endPoint != null ? endPoint.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ seq.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) packetType;
            return hashCode;
        }
    }
}