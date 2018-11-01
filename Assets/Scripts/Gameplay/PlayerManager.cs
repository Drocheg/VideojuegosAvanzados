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
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void IsAiming(bool isAiming) {
		_isAiming = isAiming;
	}

	public void Die() {
		_pickUp.enabled = false;
	}
}
