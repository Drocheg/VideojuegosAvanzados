
using System;
using System.IO;

public class Packet
{
	public uint channel;
	public ulong seq;

    public static Packet WritePacket(uint channel, BitWriter writer, uint channels)
    {
        var packet = new Packet();
        writer.WriteInt((ulong) channel, 0, channels);
        return packet;
    }

    public static Packet ReadPacket(BitReader reader, int channels, int maxSeq)
    {
        var packet = new Packet();
        packet.channel = (uint) reader.ReadInt(0, channels);
		packet.seq = (uint) reader.ReadInt(0, maxSeq);
        return packet;
    }
}