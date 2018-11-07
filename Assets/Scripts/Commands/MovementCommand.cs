public class MoveCommand {
	public float _strafe, _run, _step, _delta, _maxTime, _timeStep;
	public MoveCommand(float run, float strafe, float step, float delta, float maxTime, float timeStep) {
		_run = run;
		_strafe = strafe;
		_step = step;
		_delta = delta;
		_maxTime = maxTime;
		_timeStep = timeStep;
	}

	public void Serialize(BitWriter writer) {
		// 
		writer.WriteInt(0, 0, 1);
		writer.WriteFloat(_strafe, -1, 1,_step);
		writer.WriteFloat(_run, -1, 1, _step);
		writer.WriteFloat(_delta, 0, _maxTime, _timeStep);
	}

	public static MoveCommand Deserialize(BitReader reader, float step, float maxTime, float timeStep) {
		var strafe = reader.ReadFloat(-1, 1, step);
		var run = reader.ReadFloat(-1, 1, step);
		var delta = reader.ReadFloat(0, maxTime, timeStep);
		return new MoveCommand(run, strafe, step, delta, maxTime, timeStep);
	}
}