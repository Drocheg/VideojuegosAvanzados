using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WeaponManager : MonoBehaviour {
	public int InitialBullets;
	public int BulletClipSize;

	public TextMeshProUGUI RemainingBulletsUI, ClipBulletsUI;
	public Canvas BulletsCanvas;
	private int _bulletsLoaded;
	private int _bulletsRemaining;

	// Use this for initialization
	void Start () {
		_bulletsLoaded = BulletClipSize;
		_bulletsRemaining = InitialBullets;	
	}
	
	// Update is called once per frame
	void Update () {
		RemainingBulletsUI.text = _bulletsRemaining.ToString();
		ClipBulletsUI.text = _bulletsLoaded.ToString();
	}

	public void ReloadBullets() 
	{
		if (_bulletsRemaining > 0) {
			var reloadAmount = Mathf.Min(BulletClipSize - _bulletsLoaded, _bulletsRemaining);
			_bulletsRemaining -= reloadAmount;
			_bulletsLoaded += reloadAmount;
		}
	}

	public bool ShootIfAble()
	{
		if (_bulletsLoaded > 0) {
			_bulletsLoaded--;
			return true;
		}
		return false;
	}

	public bool CanReload()
	{
		return _bulletsRemaining > 0 && _bulletsLoaded < BulletClipSize;
	}

	public void AddBullets(int bullets) {
		_bulletsRemaining += bullets;
	}

	public void TurnOffGUI()
	{
		BulletsCanvas.enabled = false;
	}

	public void TurnOnGUI() 
	{
		BulletsCanvas.enabled = true;
	}
}
