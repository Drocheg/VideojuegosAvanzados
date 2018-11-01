using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WithinExplosionRadius : MonoBehaviour {
	private HashSet<LimbManager> _limbSet = new HashSet<LimbManager>();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void Explode(float damage) {
		foreach(var limb in _limbSet) {
			limb.TakeDamage(damage);
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
