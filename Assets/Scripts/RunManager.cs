using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour {
	private Animator _playerAnimator;
	private Transform _chest;
	// Use this for initialization
	void Start () {
		_playerAnimator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		var strafe = Input.GetAxis("Horizontal");
		var speed = Input.GetAxis("Vertical");

		_playerAnimator.SetFloat("Run", speed / 2);
		_playerAnimator.SetFloat("Strafe", strafe);
		_chest = _playerAnimator.GetBoneTransform(HumanBodyBones.Chest);
	}
}
