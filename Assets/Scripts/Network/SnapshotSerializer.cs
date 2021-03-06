using System.Collections.Generic;
using UnityEngine;

public class SnapshotSerializer : INetworkEventFactory
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
	public void Serialize(BitWriter writer)
	{
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
	}

	public INetworkEvent Deserialize(BitReader reader)
	{
		foreach(var entity in entities) {
			var changed = reader.ReadBit();
			if (changed && entity != null) {
				entity.Deserialize(reader);
			}
		}
        return null;
	}
}