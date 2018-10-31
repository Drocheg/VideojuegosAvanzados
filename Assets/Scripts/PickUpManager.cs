using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PickUpManager : MonoBehaviour {
	public Transform WorldCamera;
	public float PickUpDistance;
	public TextMeshProUGUI PickUpPrompt, PickUpItemName;
	private LayerMask mask;
	public AudioSource PickUpSound;
	public WeaponManager WeaponManager;
	// Use this for initialization
	void Start () {
		mask = LayerMask.GetMask("Default");
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		if(Physics.Raycast(WorldCamera.position, WorldCamera.forward, out hit, PickUpDistance, mask) && hit.collider.tag == "ItemPickUp") {
			var pickUp = hit.collider.GetComponent<PickUpItem>();
			if (Input.GetButton("PickUp")) {
				PickUpSound.Play();
				pickUp.PickUp(this);
			}
			PickUpItemName.text = pickUp.Name;
			PickUpItemName.enabled = true;
			PickUpPrompt.enabled = true;
		} else {
			PickUpItemName.enabled = false;
			PickUpPrompt.enabled = false;
		}
	}
}
