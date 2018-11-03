
using System;
using System.IO;
using System.Net;

public class Packet
{
    public uint channelId;
    public EndPoint endPoint;
	public ulong seq;
    public byte[] buffer;
    private Packet(uint channelId, ulong seq, byte[] buffer, EndPoint endPoint) {
        this.channelId = channelId;
        this.seq = seq;
        this.buffer = buffer;
        this.endPoint = endPoint;
    }

    public static Packet WritePacket(uint channel, ulong seq, ISerial payload, uint channels, uint maxSeq, EndPoint endPoint)
    {
        var bitWriter = new BitWriter(1000);
		bitWriter.WriteInt(1, 0, maxSeq);
		bitWriter.WriteInt(channel, 0, channels);
		payload.Serialize(bitWriter);
		bitWriter.Flush();
        return new Packet(channel, seq, bitWriter.GetBuffer().GetBuffer(), endPoint);
    }

    public static Packet ReadPacket(byte[] buffer, int channels, int maxSeq, EndPoint endPoint)
    {
        var reader = new BitReader(new MemoryStream(buffer));
        var channel = (uint) reader.ReadInt(0, channels);
		var seq = (uint) reader.ReadInt(0, maxSeq);
        return new Packet(channel, seq, buffer, endPoint);
    }
}