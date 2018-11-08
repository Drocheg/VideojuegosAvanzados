using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthCharacterEntity : CharacterEntity, IAuth {
	public int Id;
	private Animator _animator;
	private CharacterController _characterController;
	public float Speed;
	// Use this for initialization
	private AuthWorld _world;
	HealthManager _healthManager;
	void Start () {
		StartCoroutine(DelayedAddReference());
		_animator = GetComponent<Animator>();
		_characterController = GetComponent<CharacterController>();
		_world = GameObject.FindObjectOfType<AuthWorld>();
		_healthManager = GetComponent<HealthManager>();
	}

	IEnumerator DelayedAddReference() {
		yield return new WaitForEndOfFrame();
		GameObject.FindObjectOfType<AuthWorld>().AddReference(Id, this);
	}
	
	public bool IsLocalPlayer;
	public uint lastProcessedInput;
	// Update is called once per frame
	// public void UpdateEntity (float deltaTime) {
	// 	if (!IsLocalPlayer && CharacterIsMoving) {
	// 		transform.Translate(transform.forward * deltaTime * CurrentMovement.x);
	// 		transform.Translate(transform.right * deltaTime * CurrentMovement.y);
	// 	}
	// }

	public void Serialize(BitWriter writer) {
		writer.WriteFloat(transform.position.x, _world.MinPosX, _world.MaxPosX, _world.Step);
		writer.WriteFloat(transform.position.y, _world.MinPosY, _world.MaxPosY, _world.Step);
		writer.WriteFloat(transform.position.z, _world.MinPosZ, _world.MaxPosZ, _world.Step);
		writer.WriteFloat(_animator.GetFloat("Strafe"), -1, 1, _world.AnimationStep);
		writer.WriteFloat(_animator.GetFloat("Run"), -1, 1, _world.AnimationStep);
		writer.WriteFloat(transform.eulerAngles.y, 0, 360, _world.RotationStep);
		writer.WriteInt(lastProcessedInput, 0, (uint) _world.MaxMoves);
		writer.WriteFloat(Mathf.Max(_healthManager._hp, 0), 0, _world.MaxHP, 1);
	}

	public void Move(MoveCommand command) {
		var eulerAngles = transform.eulerAngles;
		eulerAngles.y = command._rot;
		transform.eulerAngles = eulerAngles;
		_characterController.Move(command._run * Speed * command._delta * transform.TransformDirection(Vector3.forward));
		_characterController.Move(command._strafe * Speed * command._delta * transform.TransformDirection(Vector3.right));
		_animator.SetFloat("Strafe", command._strafe);
		_animator.SetFloat("Run", command._run);
		lastProcessedInput = (uint) command._moveCounter;
	}

	public override int GetId(){return Id;}
}
