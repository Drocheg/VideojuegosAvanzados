public class MoveCommand {
	public float _strafe, _run, _step, _delta, _maxTime, _timeStep, _rotW, _rotY, _rotStep;
	
	
	public MoveCommand(float run, float strafe, float step, float rotW, float rotY, float rotStep, float delta, float maxTime, float timeStep) {
		_run = run;
		_strafe = strafe;
		_step = step;
		_rotW = rotW;
		_rotY = rotY;
		_rotStep = rotStep;
		_delta = delta;
		_maxTime = maxTime;
		_timeStep = timeStep;
	}

	public void Serialize(BitWriter writer) {
		// 
		writer.WriteInt(0, 0, 1);
		writer.WriteFloat(_strafe, -1, 1,_step);
		writer.WriteFloat(_run, -1, 1, _step);
		writer.WriteFloat(_rotW, -1, 1, _rotStep);
		writer.WriteFloat(_rotY, -1, 1, _rotStep);
		writer.WriteFloat(_delta, 0, _maxTime, _timeStep);
	}

	public static MoveCommand Deserialize(BitReader reader, float step, float rotStep, float maxTime, float timeStep) {
		var strafe = reader.ReadFloat(-1, 1, step);
		var run = reader.ReadFloat(-1, 1, step);
		var delta = reader.ReadFloat(0, maxTime, timeStep);
		var rotW = reader.ReadFloat(-1, 1, rotStep);
		var rotY = reader.ReadFloat(-1, 1, rotStep);
		return new MoveCommand(run, strafe, step, rotW, rotY, rotStep, delta, maxTime, timeStep);
	}
}