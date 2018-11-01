using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitchManager : MonoBehaviour {
	public List<IGenericWeaponManager> Weapons;
	private int _current = 0;
	// Use this for initialization
	void Start () {
		StartCoroutine(DelayedStart());
	}

	IEnumerator DelayedStart() {
		yield return new WaitForEndOfFrame();

		foreach(var w in Weapons) {
			w.gameObject.SetActive(false);
		}

		Weapons[_current].gameObject.SetActive(true);
		Weapons[_current].ResetAnimations();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("SwitchWeapons")) {
			Weapons[_current].TurnAnimationsOff();
			Weapons[_current].gameObject.SetActive(false);
			_current = _current + 1 == Weapons.Count ? 0 : _current + 1;
			Weapons[_current].gameObject.SetActive(true);
			Weapons[_current].ResetAnimations();
		}
	}
	
}
