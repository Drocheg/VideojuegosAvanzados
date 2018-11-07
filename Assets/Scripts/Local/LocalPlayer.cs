using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour {
	PlayerManager _player;
	LocalNetworkManager _localNetworkEntity;

	public float MaxTime, TimePrecision, MovePrecision, RotPrecision;
	// Use this for initialization
	void Start () {
		_player = GetComponent<PlayerManager>();
		_localNetworkEntity = GameObject.FindObjectOfType<LocalNetworkManager>();
	}
	
	// Update is called once per frame
	void Update () {
		var rot = transform.eulerAngles.y;
		var command = new MoveCommand(_player.Run, _player.Strafe,  MovePrecision, rot, RotPrecision, Time.deltaTime, MaxTime, TimePrecision);
		_localNetworkEntity.SendReliable(command.Serialize);
	}


}
