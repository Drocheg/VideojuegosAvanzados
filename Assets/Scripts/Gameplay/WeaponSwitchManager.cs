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
		yield return null;
		yield return null;

		foreach(var w in Weapons) {
			if (Weapons[_current] == w ) {
				Weapons[_current].gameObject.SetActive(true);
				Weapons[_current].ResetAnimations();
			} else {
				w.gameObject.SetActive(false);
			}
		}
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
