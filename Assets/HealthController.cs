using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour {

	public float InitialHP;
	private Animator _animator;
	private float _hp;
	// Use this for initialization
	void Start () {
		_hp = InitialHP;
		_animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void DeathAnimation()
	{
		_animator.SetBool("Dead", true);
		_animator.SetLayerWeight(1, 0); // Disable aiming layer
	}

	public void TakeDamage(float damage) {
		_hp -= damage;
		if (_hp <= 0) {
			DeathAnimation();
		}
	}
}
