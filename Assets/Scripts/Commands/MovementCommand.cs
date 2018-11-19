using UnityEngine;

public class MoveCommand {
	public float _strafe, _run, _step, _delta, _maxTime, _timeStep, _rot, _rotStep; 
	public ulong _moveCounter, _maxMoves;
	
	public MoveCommand(float strafe, float run, float step, float rot, float rotStep, float delta, float maxTime, float timeStep, ulong moveCounter, ulong maxMoves) {
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
		Debug.Log("Creating Move Command");
	}

	public void Serialize(BitWriter writer) {
		// 
		writer.WriteInt((uint) NetworkCommand.MOVE_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt((uint)_moveCounter, 0, (uint)_maxMoves);
		writer.WriteFloat(_strafe, -1, 1,_step);
		writer.WriteFloat(_run, -1, 1, _step);
		writer.WriteFloat(_rot, -1, 360, _rotStep);
		writer.WriteFloat(_delta, 0, _maxTime, _timeStep);
	}

	public static MoveCommand Deserialize(BitReader reader, float step, float rotStep, float maxTime, float timeStep, ulong maxMoves) {
		var moveCounter = reader.ReadInt(0, (int)maxMoves);
		var strafe = reader.ReadFloat(-1, 1, step);
		var run = reader.ReadFloat(-1, 1, step);
		var rot = reader.ReadFloat(-1, 360, rotStep);
		var delta = reader.ReadFloat(0, maxTime, timeStep);
		return new MoveCommand(strafe, run, step, rot, rotStep, delta, maxTime, timeStep, (ulong) moveCounter, maxMoves);
	}

	

	
}