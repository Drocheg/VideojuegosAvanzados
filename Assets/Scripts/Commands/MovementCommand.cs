public class MoveCommand {
	public float _strafe, _run, _step, _delta, _maxTime, _timeStep, _rot, _rotStep;
	
	
	public MoveCommand(float strafe, float run,  float step, float rot, float rotStep, float delta, float maxTime, float timeStep) {
		_run = run;
		_strafe = strafe;
		_step = step;
		_rot = rot;
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
		writer.WriteFloat(_rot, -360, 360, _rotStep);
		writer.WriteFloat(_delta, 0, _maxTime, _timeStep);
	}

	public static MoveCommand Deserialize(BitReader reader, float step, float rotStep, float maxTime, float timeStep) {
		var strafe = reader.ReadFloat(-1, 1, step);
		var run = reader.ReadFloat(-1, 1, step);
		var rot = reader.ReadFloat(-360, 360, rotStep);
		var delta = reader.ReadFloat(0, maxTime, timeStep);
		return new MoveCommand(run, strafe, step, rot, rotStep, delta, maxTime, timeStep);
	}
}