using System.Collections.Generic;

public class SnapshotSerializer
{
	private static SnapshotSerializer instance;

	public static SnapshotSerializer GetInstance()
	{
		if (instance == null) {
			instance = new SnapshotSerializer();
		}
		return instance;
	}
	public static int ENTITY_NUMBER = 2;
	private ISerial[] entities;
	private SnapshotSerializer() 
	{
		entities = new ISerial[ENTITY_NUMBER];
	}
	public void AddReference(int id, ISerial entity)
	{
		entities[id] = entity;
	}
	public Packet Serialize()
	{
		var packet = Packet.WritePacket(Packet.PacketType.SNAPSHOT);
		var writer = new BitWriter(packet.buffer);
		foreach(var entity in entities) {
			if (entity != null) {
				writer.WriteBit(true);
				entity.Serialize(writer);
			} else {
				writer.WriteBit(false);
			}
		}
		writer.Flush();
		writer.Reset();
		return packet;
	}

	public void Deserialize(Packet packet)
	{
		var reader = new BitReader(packet.buffer);
		foreach(var entity in entities) {
			var changed = reader.ReadBit();
			if (changed && entity != null) {
				entity.Deserialize(reader);
			}
		}
	}
}