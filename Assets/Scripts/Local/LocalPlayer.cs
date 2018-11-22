using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour {

	public bool Prediction;
	public float Speed;
	PlayerManager _player;
	LocalNetworkManager _localNetworkEntity;

	Queue<MoveCommand> _queuedMoves;
	CharacterController _characterController;
	Animator _animator;
	ulong _moveCounter;
	public ulong MaxMoves;
	public int MaxMoveQueueSize;
	public float MaxTime, TimePrecision, MovePrecision, RotPrecision;
	private LocalPlayer _localPlayer;
	
	// Use this for initialization
	void Start () {
		_player = GetComponent<PlayerManager>();
		_localNetworkEntity = GameObject.FindObjectOfType<LocalNetworkManager>();
		_queuedMoves = new Queue<MoveCommand>();
		_characterController = GetComponent<CharacterController>();
		_animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		var rot = transform.eulerAngles.y;

		incSeq();
		var command = new MoveCommand(_player.Strafe, _player.Run, MovePrecision, rot, RotPrecision, Time.deltaTime, MaxTime, TimePrecision, _moveCounter, MaxMoves);
		if (Prediction) {
			// Prediction code. Allow free movement.
			if (_queuedMoves.Count < MaxMoveQueueSize) {
				Move(command);
				_queuedMoves.Enqueue(command);
			} else {
				Debug.Log("Move queue filled");
			}
		}
		_localNetworkEntity.SendReliable(command.Serialize);
	}

	public void AdjustPositionFromSnapshot(Vector3 position, ulong lastProcessedInput) {
		transform.position = position;
		while (_queuedMoves.Count > 0 && IsBiggerOrEqual(lastProcessedInput, _queuedMoves.Peek()._moveCounter, MaxMoves)) {
			_queuedMoves.Dequeue();
		}
		foreach(var moveC in _queuedMoves) {
			Move(moveC);
		}
	}

	public void Move(MoveCommand command) {
		var eulerAngles = transform.eulerAngles;
		var oldRot = eulerAngles.y;
		eulerAngles.y = command._rot;
		transform.eulerAngles = eulerAngles;
		_characterController.Move(command._run * Speed * command._delta * transform.TransformDirection(Vector3.forward));
		_characterController.Move(command._strafe * Speed * command._delta * transform.TransformDirection(Vector3.right));
		_animator.SetFloat("Strafe", command._strafe);
		_animator.SetFloat("Run", command._run);
		eulerAngles.y =  oldRot;
		transform.eulerAngles = eulerAngles;
	}

	protected static ulong mod(long x, long m) {
		return (ulong)((x%m + m)%m);
	}
	
	protected static ulong MapToModule(ulong a, ulong maxRecvSeq, ulong maxSeqPossible) {
		return mod((long)a + (long)maxSeqPossible / 2 - (long)maxRecvSeq, (long)maxSeqPossible);
	}

	protected bool isBiggerThan(ulong first, ulong second, ulong max)
	{
		return MapToModule(first, second, max) > max / 2;
	}
	
	protected bool isEqualThan(ulong first, ulong second, ulong max)
	{
		return MapToModule(first, second, max) == max / 2;
	}

	protected bool IsBiggerOrEqual(ulong first, ulong second, ulong max) {
		return isBiggerThan(first, second, max) || isEqualThan(first, second, max);
	}

	protected ulong incSeq()
	{
		var oldSeq = _moveCounter;
		_moveCounter = (_moveCounter + 1) % MaxMoves;
		return oldSeq;
	}

}
