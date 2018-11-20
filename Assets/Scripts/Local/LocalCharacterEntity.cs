﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCharacterEntity : LocalEntity {
	public int Id;
	public int MinQueuedPositions, MaxQueuedPositions, TargetQueuedPositions;
	public Vector3? _previousPosition, _nextPosition;
	public Queue<Vector3DeltaTime> _queuedPositions;
	public Vector2? _previousAnimation, _nextAnimation;
	public float _previousRotation, _nextRotation, _currentRotation;
	private Animator _animator;
	private Transform _chest;
	public bool IsLocalPlayer;
	private LocalWorld _localWorld;
	private HealthManager _healthManager;

	public class Vector3DeltaTime
	{
		public Vector3 pos;
		public Vector2 animation;
		public float rot;
		public ulong lastProcessedInput;
	}


	private LocalPlayer _localPlayer;
	public void Init()
	{
		_queuedPositions = new Queue<Vector3DeltaTime>(MaxQueuedPositions);
		_animator = GetComponent<Animator>();
		_localWorld = GameObject.FindObjectOfType<LocalWorld>();
		_localPlayer = GameObject.FindObjectOfType<LocalPlayer>();
		_healthManager = GetComponent<HealthManager>();
		StartCoroutine(DelayAddReference());
	}

	IEnumerator DelayAddReference() {
		yield return new WaitForEndOfFrame();
		_localWorld.AddReference(Id, this);
	}

	public bool DequeNextPosition(out Vector3? deqPosition, out Vector2? animation, out float rot, out ulong lastProcessedInput)
	{
		if (_queuedPositions.Count > 0){
			var wrapper = _queuedPositions.Dequeue();
			deqPosition = wrapper.pos;
			animation = wrapper.animation;
			rot = wrapper.rot;
			lastProcessedInput = wrapper.lastProcessedInput;
			return true;
		}
		deqPosition = null;
		animation = null;
		rot = 0;
		lastProcessedInput = 0;
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
		if((!IsLocalPlayer || !_localPlayer.Prediction) && _previousPosition != null && _nextPosition != null) {
			transform.position = Vector3.Lerp(_previousPosition.Value, _nextPosition.Value, lerp);
			LerpAnimation(_previousAnimation.Value, _nextAnimation.Value, lerp);
			if (!IsLocalPlayer) {
				var euler = transform.eulerAngles;
				var nextRot = Quaternion.Euler(euler.x, _nextRotation, euler.z);
				var prevRot = Quaternion.Euler(euler.x, _previousRotation, euler.z);
				transform.rotation = Quaternion.Slerp(prevRot, nextRot, lerp);
			}
		}
	}

	public void QueueNextPosition(Vector3 nextPos, Vector2 anim, float rot, ulong lastProcessedInput)
	{
		while (_queuedPositions.Count >= MaxQueuedPositions) {
			_queuedPositions.Dequeue();
		}
		_queuedPositions.Enqueue(new Vector3DeltaTime() { pos = nextPos, animation = anim, rot = rot, lastProcessedInput = lastProcessedInput});
	}

	public override void Deserialize(BitReader reader) {
		Vector3 pos;
		pos.x = reader.ReadFloat(_localWorld.MinPosX, _localWorld.MaxPosX, _localWorld.Step);
		pos.y = reader.ReadFloat(_localWorld.MinPosY, _localWorld.MaxPosY, _localWorld.Step);
		pos.z = reader.ReadFloat(_localWorld.MinPosZ, _localWorld.MaxPosZ, _localWorld.Step);
		Vector2 anim;
		anim.x = reader.ReadFloat( -1, 1, _localWorld.AnimationStep);
		anim.y = reader.ReadFloat( -1, 1, _localWorld.AnimationStep);
		float rot = reader.ReadFloat(-1, 360, _localWorld.RotationStep);
		ulong lastProcessedInput = (ulong)reader.ReadInt(0, (int) _localPlayer.MaxMoves);
		_healthManager.SetHP(reader.ReadFloat(0, _localWorld.MaxHP, 0.1f));
		QueueNextPosition(pos, anim, rot, lastProcessedInput);
	}
	public override int GetId() {
		return Id;
	}

	public override EntityType GetEntityType()
	{
		return EntityType.CHARACTER;
	}
}

