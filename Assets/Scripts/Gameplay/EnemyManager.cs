using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthManager))]
[RequireComponent(typeof(Animator))]
public class EnemyManager : MonoBehaviour {
	private Animator _animator;
	private Collider _hitCollider;

	private AudioSource _HitSound;

	private HealthManager _healthManager;
	public List<Collider> Limbs;
	// Use this for initialization
	void Start () {
		_animator = GetComponent<Animator>();
		_HitSound = GetComponent<AudioSource>();
		_healthManager = GetComponent<HealthManager>();
	}
	public void Die() {
		_HitSound.Play();
		_healthManager.enabled = false;
		foreach(var collider in Limbs) {
			collider.enabled = false;
		}
	}

}