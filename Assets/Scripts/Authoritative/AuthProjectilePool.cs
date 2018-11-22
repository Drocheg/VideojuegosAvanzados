using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthProjectilePool : MonoBehaviour {
	public AuthProjectileEntity Prefab;
	private int Count;
	
	Queue<AuthProjectileEntity> _projectiles;


	// Use this for initialization
	void Start () {
		Count = GetComponent<AuthWorld>().MaxProjectiles;
		_projectiles = new Queue<AuthProjectileEntity>(Count);

		for (int i = 0; i < Count; i++) {
			_projectiles.Enqueue(Instantiate(Prefab));
		}	
	}
	
	public AuthProjectileEntity GetProjectile() {
		if (_projectiles.Count > 0) {
			var proj = _projectiles.Dequeue();
			proj.GetComponent<Projectile>().Reset();
			return proj;
		}
		return null;
	}

	public void Release(AuthProjectileEntity proj) {
		_projectiles.Enqueue(proj);
	}
}
