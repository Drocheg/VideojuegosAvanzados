using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
	public WithinExplosionRadius ExplosionRadius;
	public ParticleSystem ExplosionParticles;

	private bool _explode, _hasExploded;
	public float DestroyDelay;
	public float Damage;
	private Rigidbody _rb;
	// Use this for initialization
	void Start () {
		_rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate() {
		if (_explode && !_hasExploded) {
			_hasExploded = true;
			ExplosionParticles.Play();
			ExplosionRadius.Explode(Damage);
			_rb.isKinematic = true;
			StartCoroutine(WaitAndDestroy());
		}
	}

	IEnumerator WaitAndDestroy() {
		yield return new WaitForSeconds(DestroyDelay);
		Destroy(gameObject);
	}

	public void Explode() {
		_explode = true;
	}
}
