
using System;
using System.IO;

public class Packet
{
    public enum PacketType: uint
    {
        SNAPSHOT = 1,
    }
    public static Int32 SEQ_NUMBER;
    public Int32 SeqNumber;
    public uint packetType;
    public MemoryStream buffer;

    private Packet(){}

    public static Packet WritePacket(PacketType packetType, BitWriter writer)
    {
        var packet = new Packet();
        packet.packetType = (uint) packetType;
        packet.SeqNumber = SEQ_NUMBER++;
        writer.WriteInt((ulong) packetType, 0, (uint) Enum.GetNames(typeof(PacketType)).Length);
        writer.WriteInt((ulong) packet.SeqNumber, 0, Int32.MaxValue);
        packet.buffer = writer.GetBuffer();
        return packet;
    }

    public static Packet ReadPacket(BitReader reader)
    {
        var packet = new Packet();
        packet.packetType = (uint) reader.ReadInt(0, Enum.GetNames(typeof(PacketType)).Length);
        packet.SeqNumber = reader.ReadInt(0, Int32.MaxValue);
        packet.buffer = reader.GetBuffer();

        return packet;
    }

}