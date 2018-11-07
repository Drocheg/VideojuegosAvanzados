using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCharacterEntity : MonoBehaviour, ILocal {
	public int Id;
	public float MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step, RotationStep, AnimationStep;
	public int MinQueuedPositions, MaxQueuedPositions, TargetQueuedPositions;
	public Vector3? _previousPosition, _nextPosition;
	private Queue<Vector3DeltaTime> _queuedPositions;
	public Vector2? _previousAnimation, _nextAnimation;
	public Quaternion? _previousRotation, _nextRotation, _currentRotation;
	private Animator _animator;
	private Transform _chest;
	public bool IsLocalPlayer;
	class Vector3DeltaTime
	{
		public Vector3 pos;
		public Vector2 animation;
		public Quaternion rot;
	}

	public void Start()
	{
		_queuedPositions = new Queue<Vector3DeltaTime>(MaxQueuedPositions);
		_animator = GetComponent<Animator>();
		StartCoroutine(DelayAddReference());
	}

	IEnumerator DelayAddReference() {
		yield return new WaitForEndOfFrame();
		GameObject.FindObjectOfType<LocalWorld>().AddReference(Id, this);
	}

		public bool DequeNextPosition(out Vector3? deqPosition, out Vector2? animation, out Quaternion? rot)
	{
		if (_queuedPositions.Count > 0){
			var wrapper = _queuedPositions.Dequeue();
			deqPosition = wrapper.pos;
			animation = wrapper.animation;
			rot = wrapper.rot;
			return true;
		}
		deqPosition = null;
		animation = null;
		rot = null;
		return false;
	}

	private void LerpAnimation(Vector2 prevAnim, Vector2 nextAnim, float d)
	{
		var anim = Vector2.Lerp(prevAnim, nextAnim, d);
		_animator.SetFloat("Strafe", anim.x);
		_animator.SetFloat("Run", anim.y);
		_chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
	}

	public void UpdateEntity(float lerp) {
		transform.position = Vector3.Lerp(_previousPosition.Value, _nextPosition.Value, lerp);
		LerpAnimation(_previousAnimation.Value, _nextAnimation.Value, lerp);
		transform.rotation = Quaternion.Lerp(_previousRotation.Value, _nextRotation.Value, lerp);
	}

	public void QueueNextPosition(Vector3 nextPos, Vector2 anim, Quaternion rot)
	{
		if (_queuedPositions.Count >= MaxQueuedPositions) {
			Debug.Log("Dropping queued positions");
			_queuedPositions.Dequeue();
		}
		_queuedPositions.Enqueue(new Vector3DeltaTime() { pos = nextPos, animation = anim, rot = rot });
	}

	public void Deserialize(BitReader reader) {
		Vector3 pos;
		pos.x = reader.ReadFloat(MinPosX, MaxPosX, Step);
		pos.y = reader.ReadFloat(MinPosY, MaxPosY, Step);
		pos.z = reader.ReadFloat(MinPosZ, MaxPosZ, Step);
		Vector2 anim;
		anim.x = reader.ReadFloat( -1, 1, AnimationStep);
		anim.y = reader.ReadFloat( -1, 1, AnimationStep);
		Quaternion rot;
		if (IsLocalPlayer) {
			rot = transform.rotation;
			reader.ReadFloat(-1, 1, RotationStep);
			reader.ReadFloat(-1, 1, RotationStep);
		} else {
			rot.w = reader.ReadFloat(-1, 1, RotationStep);
			rot.x = 0;
			rot.y = reader.ReadFloat(-1, 1, RotationStep);
			rot.z = 0;
		}
		QueueNextPosition(pos, anim, rot);
	} 
}

