using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalShootManager : ShootManager {
	LocalNetworkManager _networkManager;
	LocalWorld _localWorld;
	new void Start() {
		base.Start();
		_networkManager = GameObject.FindObjectOfType<LocalNetworkManager>();
		_localWorld = GameObject.FindObjectOfType<LocalWorld>();
	}

	protected override void RegisterHit(Vector3 hit, Vector3 normal,  float damage, int id) {
		var command = new ShootCommand(damage, id, _localWorld.MaxEntities, hit.x, hit.y, hit.z, normal.x, normal.y, normal.z, _localWorld.MinPosX, _localWorld.MaxPosX, _localWorld.MinPosY, _localWorld.MaxPosY, _localWorld.MinPosZ, _localWorld.MaxPosZ, _localWorld.Step);
		_networkManager.SendReliable(command.Serialize);
	}

	protected override void RegisterDamage() {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
