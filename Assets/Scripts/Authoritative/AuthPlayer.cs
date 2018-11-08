using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthPlayer : MonoBehaviour {
	PlayerManager _playerManager;
	AuthWorld _authWorld;
	public float Speed;
	CharacterController _characterController;
	// Use this for initialization
	void Start () {
		_playerManager = GetComponent<PlayerManager>();
		_authWorld = GameObject.FindObjectOfType<AuthWorld>();
		_characterController = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
		_characterController.Move(_playerManager.Run * Speed * Time.deltaTime * transform.TransformDirection(Vector3.forward));
		_characterController.Move(_playerManager.Strafe * Speed * Time.deltaTime * transform.TransformDirection(Vector3.right));
	}
}
