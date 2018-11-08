public class ShootCommand {
	public float _cX, _cY, _cZ, _minX, _maxX, _minY, _maxY, _minZ, _maxZ, _positionPrecision;
	public bool _hitBlood;
	public ShootCommand(bool hitBlood, float cx, float cy, float cz, float minX, float maxX, float minY, float maxY, float minZ, float maxZ, float positionPrecision) {
		_hitBlood = hitBlood;
		_cX = cx;
		_cY = cy;
		_cZ = cz;
		_minX = minX;
		_maxX = maxX;
		_minY = minY;
		_maxY = maxY;
		_minZ = minZ;
		_maxZ = maxZ;
		_positionPrecision = positionPrecision;
	}

	public void Serialize(BitWriter writer) {
		// Serialize Command number
		writer.WriteInt((uint) NetworkCommand.SHOOT_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteFloat(_cX, _minX, _maxX, _positionPrecision);
		writer.WriteFloat(_cY, _minY, _maxY, _positionPrecision);
		writer.WriteFloat(_cZ, _minZ, _maxZ, _positionPrecision);
		writer.WriteBit(_hitBlood);
	}

	public static ShootCommand Deserialize(BitReader reader, float minX, float maxX, float minY, float maxY, float minZ, float maxZ, float positionPrecision) {
		var cx = reader.ReadFloat(minX, maxX, positionPrecision);
		var cy = reader.ReadFloat(minY, maxY, positionPrecision);
		var cz = reader.ReadFloat(minZ, maxZ, positionPrecision);
		var hb = reader.ReadBit();
		return new ShootCommand(hb, cx, cy, cz, minX, maxX, minY, maxY, minZ, maxZ, positionPrecision);
	}
}