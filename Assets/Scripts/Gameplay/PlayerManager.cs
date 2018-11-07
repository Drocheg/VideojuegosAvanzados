using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
	Animator _animator;

	private PickUpManager _pickUp;
	PlayerHealthManager _playerHealth;
	private bool _isAiming;
	// Use this for initialization
	void Start () {
		_pickUp = GetComponent<PickUpManager>();
		_animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		var run = Input.GetAxis("Vertical");
		var strafe = Input.GetAxis("Horizontal");

		_animator.SetFloat("Run", run);
		_animator.SetFloat("Strafe", strafe);
	}

	public void IsAiming(bool isAiming) {
		_isAiming = isAiming;
	}

	public void Die() {
		_pickUp.enabled = false;
	}
}
