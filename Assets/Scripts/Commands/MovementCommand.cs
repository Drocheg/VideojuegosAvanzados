using UnityEngine;

public class MoveCommand {
<<<<<<< HEAD
	public float _strafe, _run, _step, _delta, _maxTime, _timeStep, _rot, _rotStep; 
	public int _moveCounter, _maxMoves;
	
	public MoveCommand(float strafe, float run, float step, float rot, float rotStep, float delta, float maxTime, float timeStep, int moveCounter, int maxMoves) {
=======
	public float _strafe, _run, _step, _delta, _maxTime, _timeStep, _rot, _rotStep, _moveCounter, _maxMoves;
	
	public MoveCommand(float strafe, float run,  float step, float rot, float rotStep, float delta, float maxTime, float timeStep, int moveCounter, int maxMoves) {
>>>>>>> 7e9b7a9f7a9128b27031950aadeb76e4cb154d17
		_run = run;
		_strafe = strafe;
		_step = step;
		_rot = rot;
		_rotStep = rotStep;
		_delta = delta;
		_maxTime = maxTime;
		_timeStep = timeStep;
		_moveCounter = moveCounter;
		_maxMoves = maxMoves;
	}

	public void Serialize(BitWriter writer) {
		// 
		writer.WriteInt((uint) NetworkCommand.MOVE_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt((uint)_moveCounter, 0, (uint)_maxMoves);
		writer.WriteFloat(_strafe, -1, 1,_step);
		writer.WriteFloat(_run, -1, 1, _step);
		writer.WriteFloat(_rot, 0, 360, _rotStep);
		writer.WriteFloat(_delta, 0, _maxTime, _timeStep);
	}

	public static MoveCommand Deserialize(BitReader reader, float step, float rotStep, float maxTime, float timeStep, int maxMoves) {
		var moveCounter = reader.ReadInt(0, maxMoves);
		var strafe = reader.ReadFloat(-1, 1, step);
		var run = reader.ReadFloat(-1, 1, step);
		var rot = reader.ReadFloat(0, 360, rotStep);
		var delta = reader.ReadFloat(0, maxTime, timeStep);
		return new MoveCommand(strafe, run, step, rot, rotStep, delta, maxTime, timeStep, moveCounter, maxMoves);
	}
}