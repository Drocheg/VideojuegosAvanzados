using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeManager : IGenericWeaponManager {
	public Camera GunCamera;
	public PlayerManager PlayerManager;
	private Animator _playerAnimator;
	public Camera Camera;
	public float _timeSinceLastShot;
	public float ShootingTimeout;
	private ParticlePool _sparklesPool;
	private ParticlePool _bloodPool;
	private LayerMask _mask;
	public float Range;
	public AudioClip BodyHitSound;
	public AudioClip SurfaceHitSound;
	public float HitDelay;
	private AudioSource _audioSource;
	public AudioClip Swoosh;
	public float FOVAdjustment;
	public Vector3 GunCameraAdjustment;
	private Quaternion _rot;
	public float DamageMultiplier;
	// Use this for initialization
	void Start () {
		_playerAnimator = PlayerManager.GetComponent<Animator>();
		var particlePools = GetComponents<ParticlePool>();
		_sparklesPool = particlePools[0];
		_bloodPool = particlePools[1];
		_mask = LayerMask.GetMask("Default", "Zombie", "Door");
		_audioSource = GetComponent<AudioSource>();
		_rot = Quaternion.Euler(GunCameraAdjustment);
	}
	
	// Update is called once per frame
	void Update () {
		_timeSinceLastShot += Time.deltaTime;

		if (Input.GetButton("Shoot")) {
			if (_timeSinceLastShot >= ShootingTimeout) {
				_timeSinceLastShot = 0;
				_playerAnimator.SetTrigger("Hit");
				StartCoroutine(Shoot());
			}
		} 
	}

	private IEnumerator Shoot() 
	{
		yield return new WaitForSeconds(HitDelay);
		// Draw a raycast and collision effect
		RaycastHit hit;
		if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, Range, _mask)) {
			ParticlePool particlePool;
			// Check if another player was hit;
			if (hit.collider.tag == "CharacterCollision") {
				// Make other player take damage
				var limbController = hit.collider.GetComponent<LimbManager>();
				_audioSource.clip = BodyHitSound;
				_audioSource.Play();
				limbController.TakeDamage(DamageMultiplier);
				particlePool = _bloodPool;
			} else {
				_audioSource.clip = SurfaceHitSound;
				_audioSource.Play();
				particlePool = _sparklesPool;
			}
			var particleSystem = particlePool.GetParticleSystem();
			particleSystem.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
			particleSystem.Play();
			particlePool.ReleaseParticleSystem(particleSystem);
		} else {
			_audioSource.clip = Swoosh;
			_audioSource.Play();
		}
	}

	float _oldFOV;
	Quaternion _originalRotation;
	public override void ResetAnimations()
	{
		GunCamera.transform.Rotate(GunCameraAdjustment, 20f);
		_playerAnimator.SetLayerWeight(2, 1);
		_oldFOV = GunCamera.fieldOfView;
		GunCamera.fieldOfView = FOVAdjustment;
	}

	public override void TurnAnimationsOff()
	{	
		GunCamera.fieldOfView = _oldFOV;
		_playerAnimator.SetLayerWeight(2, 0);
		Debug.Log(-GunCameraAdjustment);
		GunCamera.transform.Rotate(GunCameraAdjustment, -20f);
	}

}
