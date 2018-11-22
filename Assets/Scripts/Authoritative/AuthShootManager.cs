using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthShootManager : ShootManager {

	AuthWorld _authWorld;

	new void Start() {
		base.Start();
		_authWorld = GameObject.FindObjectOfType<AuthWorld>();
	}

	protected override void RegisterHit(Vector3 hit, Vector3 normal,  float damage, int id) {
		var command = new ShootCommand(damage, id, _authWorld.MaxEntities, hit.x, hit.y, hit.z, normal.x, normal.y, normal.z, _authWorld.MinPosX, _authWorld.MaxPosX, _authWorld.MinPosY, _authWorld.MaxPosY, _authWorld.MinPosZ, _authWorld.MaxPosZ, _authWorld.Step);
		_authWorld.Shoot(0, command);
	}
}
