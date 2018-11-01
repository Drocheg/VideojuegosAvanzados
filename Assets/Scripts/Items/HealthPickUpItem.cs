using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickUpItem : PickUpItem {

	public override void PickUp(PickUpManager manager) {
		manager.GetComponent<PlayerHealthManager>().AddHealthPack();
		Destroy(gameObject);
	}
}
