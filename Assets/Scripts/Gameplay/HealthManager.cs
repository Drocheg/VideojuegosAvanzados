using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour {

	public float InitialHP;
	private Animator _animator;
	public float _hp;
	public bool Dead;
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
		_animator.SetLayerWeight(1, 0);
		Dead = true;
	}

	private void AliveAnimation()
	{
		_animator.SetBool("Dead", false);
		_animator.SetLayerWeight(1, 0.8f);
		Dead = false;
	}

	public void TakeDamage(float damage) {
		if (Dead) return;
		_hp -= damage;
		if (_hp <= 0) {
			DeathAnimation();
		}
	}

	public void SetHP(float hp) {
		_hp = hp;
		if (_hp <= 0 && !Dead) {
			DeathAnimation();
		}
		if (_hp > 0 && Dead) {
			AliveAnimation();
		}
	}
}
