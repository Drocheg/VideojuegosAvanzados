using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollider : MonoBehaviour {
	public Projectile ParentProjectile;

	private int _layer;

	void Start() {
		_layer = LayerMask.NameToLayer("Default");
	}
	void OnTriggerEnter(Collider collider) {
		if (collider.gameObject.layer == _layer) {
			ParentProjectile.Explode();
		}
	}
}
