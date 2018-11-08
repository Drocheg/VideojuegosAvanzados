using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalShootManager : ShootManager {
	LocalNetworkManager _networkManager;

	new void Start() {
		base.Start();
		_networkManager = GameObject.FindObjectOfType<LocalNetworkManager>();
	}

	protected override void RegisterHit(Collider collider) {
	}

	protected override void RegisterDamage() {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
