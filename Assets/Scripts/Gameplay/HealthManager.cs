using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour {

	public float InitialHP;
	private Animator _animator;
	private float _hp;
	private EnemyManager _enemyManager;
	// Use this for initialization
	void Start () {
		_hp = InitialHP;
		_animator = GetComponent<Animator>();
		_enemyManager = GetComponent<EnemyManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void DeathAnimation()
	{
		_animator.SetTrigger("Dead");
		_animator.SetLayerWeight(1, 0);
		_enemyManager.Die();
	}

	public void TakeDamage(float damage) {
		_hp -= damage;
		if (_hp <= 0) {
			DeathAnimation();
		}
	}
}
