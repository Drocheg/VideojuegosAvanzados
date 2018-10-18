using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour {
	private Animator _playerAnimator;
	// Use this for initialization
	void Start () {
		_playerAnimator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		var strafe = Input.GetAxis("Horizontal");
		var speed = Input.GetAxis("Vertical");

		_playerAnimator.SetFloat("Speed", speed);
		_playerAnimator.SetFloat("Strafe", strafe);
	}

}
