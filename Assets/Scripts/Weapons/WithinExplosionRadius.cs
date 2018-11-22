using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WithinExplosionRadius : MonoBehaviour {
	private HashSet<LimbManager> _limbSet = new HashSet<LimbManager>();
	
	public void Explode(float damage, AuthWorld authWorld) {
		foreach(var limb in _limbSet) {
			bool died = limb.HealthManager.TakeDamageAndDie(limb.TakeDamage(damage));
			if (died) {
				Debug.Log("Reviving...");
				authWorld.Revive(limb.HealthManager);
			}
		}
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.tag == "CharacterCollider") {
			_limbSet.Add(collider.GetComponent<LimbManager>());
		}
	}

	void OnTriggerExit(Collider collider) {
		if (collider.tag == "CharacterCollider") {
			_limbSet.Remove(collider.GetComponent<LimbManager>());
		}
	}
}
