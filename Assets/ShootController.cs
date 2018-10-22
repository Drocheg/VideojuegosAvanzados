using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootController : MonoBehaviour {
	private Animator _playerAnimator;
	private Transform _rightShoulder;
	private Transform _leftShoulder;
	

	public AudioSource Weapon;
	public Camera Camera;
	public float _timeSinceLastShot;
	public float ShootingTimeout;
	public ParticleSystem MuzzleFlash;
	private ParticlePool _particlePool;
	// Use this for initialization
	void Start () {
		_playerAnimator = GetComponent<Animator>();
		_particlePool = GetComponent<ParticlePool>();
	}
	

	// Update is called once per frame
	void Update () {
		_timeSinceLastShot += Time.deltaTime;

		if (_timeSinceLastShot >= ShootingTimeout) {
		 if (Input.GetMouseButton(0)) {
				Shoot();
				
			}
		}
			if (Input.GetMouseButtonUp(0)) {
				_playerAnimator.SetBool("Shooting", false);
			}
	}

	private void Shoot() 
	{
		_timeSinceLastShot = 0;
		_playerAnimator.SetBool("Shooting", true);
		Weapon.Play();
		MuzzleFlash.Play();

		// Draw a raycast and collision effect
		RaycastHit hit;
		if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, 1000)) {
			var particleSystem = _particlePool.GetParticleSystem();
			particleSystem.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
			particleSystem.Play();
			_particlePool.ReleaseParticleSystem(particleSystem);
		}
	}
}
