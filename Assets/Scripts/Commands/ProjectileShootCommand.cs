using UnityEngine;

public class ProjectileShootCommand {
	public float _x, _y, _z;
	public float _minX, _minY, _minZ, _maxX, _maxY, _maxZ, _positionPrecision;
	public float _dirX, _dirY, _dirZ;
	public int _id, _maxId;

	public ProjectileShootCommand(int id, int maxId, float x, float y, float z, float dirX, float dirY, float dirZ, float minX, float maxX, float minY, float maxY, float minZ, float maxZ, float positionPrecision) {
		_x = x;
		_y = y;
		_z = z; 
		_dirX = dirX;
		_dirY = dirY;
		_dirZ = dirZ;
		_minX = minX;
		_maxX = maxX;
		_minY = minY;
		_maxY = maxY;
		_minZ = minZ;
		_maxZ = maxZ;
		_positionPrecision = positionPrecision;
		_id = id;
		_maxId = maxId;
	}

	public void Serialize(BitWriter writer) {
		// Serialize Command number
		writer.WriteInt((uint) NetworkCommand.PROJECTILE_SHOOT_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt((uint) _id, 0, (uint) _maxId);
		writer.WriteFloat(_x, _minX, _maxX, _positionPrecision);
		writer.WriteFloat(_y, _minY, _maxY, _positionPrecision);
		writer.WriteFloat(_z, _minZ, _maxZ, _positionPrecision);
		writer.WriteFloat(_dirX, _minX, _maxX, _positionPrecision);
		writer.WriteFloat(_dirY, _minY, _maxY, _positionPrecision);
		writer.WriteFloat(_dirZ, _minZ, _maxZ, _positionPrecision);
	}

	public static ProjectileShootCommand Deserialize(BitReader reader, int maxId, float minX, float maxX, float minY, float maxY, float minZ, float maxZ, float positionPrecision) {
		var id = reader.ReadInt(0, maxId);
		var x = reader.ReadFloat(minX, maxX, positionPrecision);
		var y = reader.ReadFloat(minY, maxY, positionPrecision);
		var z = reader.ReadFloat(minZ, maxZ, positionPrecision);
		var dirX = reader.ReadFloat(minX, maxX, positionPrecision);
		var dirY = reader.ReadFloat(minY, maxY, positionPrecision);
		var dirZ = reader.ReadFloat(minZ, maxZ, positionPrecision);
		return new ProjectileShootCommand(id, maxId, x, y, z, dirX, dirY, dirZ, minX, maxX, minY, maxY, minZ, maxZ, positionPrecision);
	}
}
