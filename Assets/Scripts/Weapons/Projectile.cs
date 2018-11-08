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
	// Use this for initialization
	void Start () {
		_rb = GetComponent<Rigidbody>();
		_renderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate() {
		if (_explode && !_hasExploded) {
			_hasExploded = true;
			ExplosionParticles.Play();
			if (IsAuth) {
				ExplosionRadius.Explode(Damage);
			} 
			_rb.isKinematic = true;
			_renderer.enabled = false;
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
