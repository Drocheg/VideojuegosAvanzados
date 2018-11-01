using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadManager : MonoBehaviour {
	public Canvas ReloadCanvas;
	public Image ReloadImage;
	public AudioClip Reload;
	public float ReloadFillPerSecond;
	private WeaponManager _weaponManager;
	private bool _reloading;
	private float _rFill;
	private AudioSource _audioSource;
	// Use this for initialization
	void Start () {
		_rFill = 0;
		_weaponManager = GetComponent<WeaponManager>();
		_audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!_reloading && _weaponManager.CanReload() && Input.GetButtonDown("Reload")) {
			// Start reloading
			_reloading = true;
			ReloadCanvas.enabled = true;
			_audioSource.clip = Reload;
			_audioSource.Play();
		}
		if (_reloading) {
			_rFill += ReloadFillPerSecond * Time.deltaTime;
			if (_rFill >= 1) {
				// Finished reloading.
				StopReload();
				_weaponManager.ReloadBullets();
			}  else {
				ReloadImage.fillAmount = _rFill ;
			}
		}
	}

	public bool IsReloading() {
		return _reloading;
	}

	public void StopReload() {
		_rFill = 0;
		_reloading = false;
		ReloadCanvas.enabled = false;
	}
}
