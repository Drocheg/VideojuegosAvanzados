using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
	public WithinExplosionRadius ExplosionRadius;
	public ParticleSystem ExplosionParticles;

	private bool _explode, _hasExploded;
	public bool IsAuth;
	public float DestroyDelay;
	public float Damage;
	private Rigidbody _rb;
	private Renderer _renderer;
	private AuthWorld _authWorld;
	private AuthProjectileEntity _authProjectile;

	public float ExplosionTime;
	// Use this for initialization
	void Start () {
		_rb = GetComponent<Rigidbody>();
		_renderer = GetComponent<Renderer>();
		_authProjectile = GetComponent<AuthProjectileEntity>();
		_authWorld = GameObject.FindObjectOfType<AuthWorld>();
	}

	public void Reset() {
		if (IsAuth) {
			_renderer.enabled = true;
			_rb.isKinematic = false;
			_hasExploded = false;
			_explode = false;
			StartCoroutine(ExplosionDelay());
		}
	}
	
	IEnumerator ExplosionDelay() {
		yield return new WaitForSeconds(ExplosionTime);
		Explode();
	}

	void LateUpdate() {
		if (_explode && !_hasExploded) {
			_hasExploded = true;
			ExplosionParticles.Play();
			if (IsAuth) {
				_authWorld.Explode(_authProjectile, ExplosionRadius, Damage);
			} 
			_rb.isKinematic = true;
			_renderer.enabled = false;
			StartCoroutine(WaitAndDestroy());
		}
	}

	IEnumerator WaitAndDestroy() {
		yield return new WaitForSeconds(DestroyDelay);
		ExplosionParticles.Stop();
	}

	public void Explode() {
		_explode = true;
	}
}
