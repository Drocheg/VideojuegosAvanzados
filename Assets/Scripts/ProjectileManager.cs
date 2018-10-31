using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ShootManager {
	public Rigidbody Projectile;
	public float ExplosionMagnitude;
	public Transform ProjectilePositionOrigin;

	protected override void Shoot() {
		if (!_weaponManager.ShootIfAble()) {
			if (!_audioSource.isPlaying) {
				Play(ClipEmpty);	
			}
			return;
		}
		_timeSinceLastShot = 0;
		_playerAnimator.SetBool("Shooting", true);
		Play(WeaponFire);
		MuzzleFlash.Play();

		var go = Instantiate(Projectile);
		go.transform.position = ProjectilePositionOrigin.position;
		go.AddForce(Camera.transform.forward * ExplosionMagnitude, ForceMode.Impulse);
	}
}
