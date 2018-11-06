﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthCharacterEntity : MonoBehaviour, IAuth {
	public float MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step, RotationStep, AnimationStep;
	public int Id;
	private Animator _animator;
	// Use this for initialization
	void Start () {
		StartCoroutine(DelayedAddReference());
		_animator = GetComponent<Animator>();
	}

	IEnumerator DelayedAddReference() {
		yield return new WaitForEndOfFrame();
		GameObject.FindObjectOfType<AuthWorld>().AddReference(Id, this);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Serialize(BitWriter writer) {
		writer.WriteFloat(transform.position.x, MinPosX, MaxPosX, Step);
		writer.WriteFloat(transform.position.y, MinPosY, MaxPosY, Step);
		writer.WriteFloat(transform.position.z, MinPosZ, MaxPosZ, Step);
		writer.WriteFloat(_animator.GetFloat("Strafe"), -1, 1, AnimationStep);
		writer.WriteFloat(_animator.GetFloat("Run"), -1, 1, AnimationStep);
		writer.WriteFloat(transform.rotation.w, -1, 1, RotationStep);
		writer.WriteFloat(transform.rotation.y, -1, 1, RotationStep);
	}
}
