using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ShootManager {
	public Rigidbody Projectile;
	public float ExplosionMagnitude;
	public Transform ProjectilePositionOrigin;
	public bool IsAuth;
	private AuthWorld _authWorld;
	new void Start() {
		base.Start();
		_authWorld = GameObject.FindObjectOfType<AuthWorld>();
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
		var command = new ProjectileShootCommand(
			transform.position.x,
			transform.position.y,
			transform.position.z,
			dir.x,
			dir.y,
			dir.z,
			_authWorld.MinPosX,
			_authWorld.MaxPosX,
			_authWorld.MinPosY,
			_authWorld.MaxPosY,
			_authWorld.MinPosZ,
			_authWorld.MaxPosZ, 
			_authWorld.TimePrecision);
		if (IsAuth) {
			_authWorld.NewProjectile(command);
		} else {
			// Send ProjectileShootCommand
		}

		StartCoroutine(Recoil());
	}
}
