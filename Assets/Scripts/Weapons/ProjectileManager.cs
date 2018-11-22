using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ShootManager {
	public Rigidbody Projectile;
	public float ExplosionMagnitude;
	public Transform ProjectilePositionOrigin;
	public bool IsAuth;
	private AuthWorld _authWorld;
	private LocalWorld _localWorld;
	new void Start() {
		base.Start();
		if (IsAuth) {
			_authWorld = GameObject.FindObjectOfType<AuthWorld>();
		} else {
			_localWorld = FindObjectOfType<LocalWorld>();
		}
	}
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

		var dir = Camera.transform.forward;
		if (IsAuth) {
			_authWorld.NewProjectile(0, transform.position, dir);
		} else {
			// Send ProjectileShootCommand
			_localWorld.ShootProjectile(transform.position, dir);
		}

		StartCoroutine(Recoil());
	}
}
