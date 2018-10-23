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

	private ParticlePool _sparklesPool;
	private ParticlePool _bloodPool;
	private LineRenderer _lineRenderer;
	// Use this for initialization
	void Start () {
		_playerAnimator = GetComponent<Animator>();
		var particlePools = GetComponents<ParticlePool>();
		_sparklesPool = particlePools[0];
		_bloodPool = particlePools[1];
		_lineRenderer = GetComponent<LineRenderer>();
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

	private IEnumerator DrawLineTrail(Vector3 opos, Vector3 dpos)
	{
		_lineRenderer.enabled = true;
		_lineRenderer.SetPosition(0, opos);
		_lineRenderer.SetPosition(1, dpos);
		// wait two frames
		yield return null;
		yield return null;
		_lineRenderer.enabled = false;
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
			ParticlePool particlePool;
			// Check if another player was hit;
			if (hit.collider.tag == "CharacterCollider") {
				// Make other player take damage
				var limbController = hit.collider.GetComponent<LimbController>();
				limbController.TakeDamage();
				particlePool = _bloodPool;
			} else {
				particlePool = _sparklesPool;
			}
			var particleSystem = particlePool.GetParticleSystem();
			particleSystem.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
			particleSystem.Play();
			particlePool.ReleaseParticleSystem(particleSystem);
			StartCoroutine(DrawLineTrail(Camera.transform.position, hit.point));
		} else {
			StartCoroutine(DrawLineTrail(Camera.transform.position, Camera.transform.TransformPoint(Vector3.forward * 1000)));
		}
	}
}
