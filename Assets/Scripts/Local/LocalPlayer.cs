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
	int moveCounter;
	public int MaxMoves;
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

		moveCounter++;
		var command = new MoveCommand(_player.Strafe, _player.Run, MovePrecision, rot, RotPrecision, Time.deltaTime, MaxTime, TimePrecision, moveCounter, MaxMoves);
		if (Prediction) {
			// Prediction code. Allow free movement.
			Move(command);
			_queuedMoves.Enqueue(command);
		}
		_localNetworkEntity.SendReliable(command.Serialize);
	}

	public void AdjustPositionFromSnapshot(Vector3 position, int lastProcessedInput) {
		transform.position = position;
		while (_queuedMoves.Count > 0 && _queuedMoves.Peek()._moveCounter <= lastProcessedInput) {
			_queuedMoves.Dequeue();
		}
		foreach(var moveC in _queuedMoves) {
			Move(moveC);
		}
	}

	public void Move(MoveCommand command) {
		var eulerAngles = transform.eulerAngles;
		eulerAngles.y = command._rot;
		transform.eulerAngles = eulerAngles;
		_characterController.Move(command._run * Speed * command._delta * transform.TransformDirection(Vector3.forward));
		_characterController.Move(command._strafe * Speed * command._delta * transform.TransformDirection(Vector3.right));
		_animator.SetFloat("Strafe", command._strafe);
		_animator.SetFloat("Run", command._run);
	}

}
