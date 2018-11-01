using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickUpItem : PickUpItem {

	public int BulletCount;
	public PickUpManager.WeaponAmmo AmmoType;
	public override void PickUp(PickUpManager pickUpManager) {
		pickUpManager.weaponManagers[(int)AmmoType].AddBullets(BulletCount);
		Destroy(gameObject);
	}
}
