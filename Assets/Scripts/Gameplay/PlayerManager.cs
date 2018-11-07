using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
	Animator _animator;

	private PickUpManager _pickUp;
	PlayerHealthManager _playerHealth;
	private bool _isAiming;
	// Use this for initialization
	private AuthCharacterEntity _authCharacterEntity;
	void Start () {
		_pickUp = GetComponent<PickUpManager>();
		_animator = GetComponent<Animator>();
		_authCharacterEntity = GetComponent<AuthCharacterEntity>();
	}
	
	// Update is called once per frame
	void Update () {
		Run = Input.GetAxis("Vertical");
		Strafe = Input.GetAxis("Horizontal");

		_animator.SetFloat("Run", Run);
		_animator.SetFloat("Strafe", Strafe);
	}

	public void IsAiming(bool isAiming) {
		_isAiming = isAiming;
	}

	public float Run, Strafe;

	public void Die() {
		_pickUp.enabled = false;
	}
}
