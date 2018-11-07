using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour {
	Animator _animator;
	LocalNetworkManager _localNetworkEntity;

	public float MaxTime, TimePrecision, MovePrecision;
	// Use this for initialization
	void Start () {
		_animator = GetComponent<Animator>();
		_localNetworkEntity = GameObject.FindObjectOfType<LocalNetworkManager>();
	}
	
	// Update is called once per frame
	void Update () {

		var command = new MoveCommand(_animator.GetFloat("Run"), _animator.GetFloat("Strafe"), MovePrecision, Time.deltaTime, MaxTime, TimePrecision);
		_localNetworkEntity.SendReliable(command.Serialize);
	}


}
