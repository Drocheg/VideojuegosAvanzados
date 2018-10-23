using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbController : MonoBehaviour {
	public PlayerController Player;
	public float LimbDamage;
	private HealthController _healthController;
	// Use this for initialization
	void Start () {
		_healthController = Player.GetComponent<HealthController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void TakeDamage()
	{
		_healthController.TakeDamage(LimbDamage);
	}
}
