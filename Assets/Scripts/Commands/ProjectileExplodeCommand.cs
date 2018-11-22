using UnityEngine;

public class ProjectileExplodeCommand {
	public Vector3 pos, nor, minPos, minDir, maxPos, maxDir;
	public float positionPrecision, directionPrecision;
	public int id, maxId;

	public void Serialize(BitWriter writer) {
		// Serialize Command number
		writer.WriteInt((uint) NetworkCommand.PROJECTILE_EXPLODE_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt((uint) id, 0, (uint) maxId);
		writer.WriteFloat(pos.x, minPos.x, maxPos.x, positionPrecision);
		writer.WriteFloat(pos.y, minPos.y, maxPos.y, positionPrecision);
		writer.WriteFloat(pos.z, minPos.z, maxPos.z, positionPrecision);
		writer.WriteFloat(nor.x, minDir.x, maxDir.x, directionPrecision);
		writer.WriteFloat(nor.y, minDir.y, maxDir.y, directionPrecision);
		writer.WriteFloat(nor.z, minDir.z, maxDir.z, directionPrecision);
	}

	public static ProjectileExplodeCommand Deserialize(
		BitReader reader, 
		int maxId, 
		float positionPrecision, 
		float directionPrecision, 
		Vector3 minPos, 
		Vector3 maxPos, 
		Vector3 minDir, 
		Vector3 maxDir
	) {
		var id = reader.ReadInt(0, maxId);
		Vector3 pos = new Vector3();
		pos.x = reader.ReadFloat(minPos.x, maxPos.x, positionPrecision);
		pos.y = reader.ReadFloat(minPos.y, maxPos.y, positionPrecision);
		pos.z = reader.ReadFloat(minPos.z, maxPos.z, positionPrecision);
		Vector3 dir = new Vector3();
		dir.x = reader.ReadFloat(minDir.x, maxDir.x, positionPrecision);
		dir.y = reader.ReadFloat(minDir.y, maxDir.y, positionPrecision);
		dir.z = reader.ReadFloat(minDir.z, maxDir.z, positionPrecision);
		return new ProjectileExplodeCommand(){
			pos = pos,
			nor = dir,
			minPos = minPos,
			maxPos = maxPos,
			minDir = minDir,
			maxDir = maxDir,
			positionPrecision = positionPrecision,
			id = id,
			maxId = maxId,
		};
	}
}