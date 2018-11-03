using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthCharacterEntity : MonoBehaviour, IAuth {
	public float Min, Max, Step, TimeStep, MaxTime;
	private Animator _animator;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Serialize(BitWriter writer) {
		writer.WriteFloat(transform.position.x, Min, Max, Step);
		writer.WriteFloat(transform.position.y, Min, Max, Step);
		writer.WriteFloat(transform.position.z, Min, Max, Step);
		writer.WriteFloat(_animator.GetFloat("Strafe"), -1, 1, 0.1f);
		writer.WriteFloat(_animator.GetFloat("Speed"), -1, 1, 0.1f);
		writer.WriteFloat(transform.rotation.w, -1, 1, 0.01f);
		writer.WriteFloat(transform.rotation.y, -1, 1, 0.01f);
	}
}
