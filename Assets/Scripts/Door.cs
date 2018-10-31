using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
	private HingeJoint _hinge;
	private bool _opened;
	private AudioSource _openDoorSound;
	// Use this for initialization
	void Start () {
			_hinge = GetComponent<HingeJoint>();
			_openDoorSound = GetComponent<AudioSource>();
			_opened = false;
	}

	public void Interact () {
		_openDoorSound.Play();
		if (!_opened) {
			var hinge = _hinge.spring;
			hinge.targetPosition = -90;
			_hinge.spring = hinge;
			_opened = true;
		} else {
			var hinge = _hinge.spring;
			hinge.targetPosition = 0;
			_hinge.spring = hinge;
			_opened = false;
		}
	}

	public string Prompt() {
		return _opened ? "Close" : "Open";
	}
}
