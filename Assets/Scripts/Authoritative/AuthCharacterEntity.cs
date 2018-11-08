using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthCharacterEntity : MonoBehaviour, IAuth {
	public float MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step, RotationStep, AnimationStep;
	public int Id;
	private Animator _animator;
	private CharacterController _characterController;
	public float Speed;
	// Use this for initialization
	public void Init () {
		StartCoroutine(DelayedAddReference());
		_animator = GetComponent<Animator>();
		_characterController = GetComponent<CharacterController>();
	}

	IEnumerator DelayedAddReference() {
		yield return new WaitForEndOfFrame();
		GameObject.FindObjectOfType<AuthWorld>().AddReference(Id, this);
	}
	
	public bool IsLocalPlayer;

	// Update is called once per frame
	// public void UpdateEntity (float deltaTime) {
	// 	if (!IsLocalPlayer && CharacterIsMoving) {
	// 		transform.Translate(transform.forward * deltaTime * CurrentMovement.x);
	// 		transform.Translate(transform.right * deltaTime * CurrentMovement.y);
	// 	}
	// }

	public void Serialize(BitWriter writer) {
		writer.WriteFloat(transform.position.x, MinPosX, MaxPosX, Step);
		writer.WriteFloat(transform.position.y, MinPosY, MaxPosY, Step);
		writer.WriteFloat(transform.position.z, MinPosZ, MaxPosZ, Step);
		writer.WriteFloat(_animator.GetFloat("Strafe"), -1, 1, AnimationStep);
		writer.WriteFloat(_animator.GetFloat("Run"), -1, 1, AnimationStep);
		writer.WriteFloat(transform.eulerAngles.y, 0, 360, RotationStep);
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
