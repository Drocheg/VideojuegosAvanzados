using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void DamageTaker();

[RequireComponent (typeof(AimManager))]
public class ShootManager : IGenericWeaponManager {
	protected Animator _playerAnimator;
	public AudioClip WeaponFire;
	public AudioClip ClipEmpty;
	public Camera Camera;
	public Camera GunCamera;
	public float _timeSinceLastShot;
	public float ShootingTimeout;
	public ParticleSystem MuzzleFlash;
	public PlayerManager PlayerManager;
	protected ParticlePool _sparklesPool;
	protected ParticlePool _bloodPool;
	protected AimManager _aimManager;
	protected WeaponManager _weaponManager;
	protected ReloadManager _reloadManager;
	private LayerMask _mask;
	public int AnimationLayer;
	public Vector3 GunCameraAdjustment;
	protected AudioSource _audioSource;
	public float DamageMultiplier;
	// Use this for initialization
	public void Start () {
		_playerAnimator = PlayerManager.GetComponent<Animator>();
		var particlePools = GetComponents<ParticlePool>();
		_sparklesPool = particlePools[0];
		_bloodPool = particlePools[1];
		_aimManager = GetComponent<AimManager>();
		_weaponManager = GetComponent<WeaponManager>();
		_mask = LayerMask.GetMask("Default", "Zombie", "Door");
		_reloadManager = GetComponent<ReloadManager>();
		_audioSource = GetComponent<AudioSource>();
	}
	

	// Update is called once per frame
	void LateUpdate () {
		_timeSinceLastShot += Time.deltaTime;
		_aimManager.UpdateAim();
		if (Input.GetButton("Shoot") && !_reloadManager.IsReloading()) {
			if (_timeSinceLastShot >= ShootingTimeout) {
				Shoot();
			}
		} 
		if (Input.GetButtonUp("Shoot")) {
			_playerAnimator.SetBool("Shooting", false);
		}
	}

	protected virtual void RegisterHit(Vector3 hit, Vector3 normal,  float damage, int id) {

	}

	protected virtual void RegisterDamage() {

	}

	protected virtual void Shoot() 
	{
		if (!_weaponManager.ShootIfAble()) {
			if (!_audioSource.isPlaying) {
				Play(ClipEmpty);	
			}
			return;
		}
		_timeSinceLastShot = 0;
		_playerAnimator.SetBool("Shooting", true);
		Play(WeaponFire);
		MuzzleFlash.Play();

		// Draw a raycast and collision effect
		RaycastHit hit;
		if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, 1000, _mask)) {
			ParticlePool particlePool;
			float damage = 0;
			int id = -1;
			// Check if another player was hit;
			if (hit.collider.tag == "CharacterCollider") {
				// Make other player take damage
				var limbController = hit.collider.GetComponent<LimbManager>();
				id = limbController.HealthManager.GetComponent<Entity>().GetId();
				damage = limbController.TakeDamage(DamageMultiplier);
				particlePool = _bloodPool;
			} else {
				particlePool = _sparklesPool;
			}

			RegisterHit(hit.point, hit.normal, damage, id);
		}
		StartCoroutine(Recoil());
	}

	public float RecoilAmount;
	public float RecoilSmoothness;
	protected IEnumerator Recoil()
	{
		Camera.transform.Rotate(Vector3.left * RecoilAmount * RecoilSmoothness);
		for (int i = 1; i <= RecoilAmount; i++) {
			yield return null;
			Camera.transform.Rotate(Vector3.left * (RecoilAmount - i) * RecoilSmoothness);
		}
	}
	public float FOVAdjustment;
	float _oldFOV;
	public float GunCameraAdjustmentAngle;
	public override void ResetAnimations()
	{
		GunCamera.transform.Rotate(GunCameraAdjustment, GunCameraAdjustmentAngle);
		_weaponManager.TurnOnGUI();
		_playerAnimator.SetLayerWeight(AnimationLayer, 1);
		_oldFOV = GunCamera.fieldOfView;
		GunCamera.fieldOfView = FOVAdjustment;
	}

	protected void Play(AudioClip clip) 
	{
		_audioSource.clip = clip;
		_audioSource.Play();
	}

	public override void TurnAnimationsOff()
	{
			GunCamera.fieldOfView = _oldFOV;
		_playerAnimator.SetLayerWeight(AnimationLayer, 0);
		_weaponManager.TurnOffGUI();
		_reloadManager.StopReload();
		_aimManager.ResetAiming();
		GunCamera.transform.Rotate(GunCameraAdjustment, -GunCameraAdjustmentAngle);
	}
}
