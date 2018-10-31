using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionManager : MonoBehaviour {

	public Transform WorldCamera;
	public float Distance;
	public TextMeshProUGUI OpenDoorPrompt;
	private LayerMask mask;
	
	// Use this for initialization
	void Start () {
		mask = LayerMask.GetMask("Default", "Door");
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		if(Physics.Raycast(WorldCamera.position, WorldCamera.forward, out hit, Distance, mask) && hit.collider.tag == "Interactable") {
			var door = hit.collider.GetComponent<Door>();
			if (Input.GetButtonDown("PickUp")) {
				door.Interact();
			} else {
				OpenDoorPrompt.text = door.Prompt();
				OpenDoorPrompt.enabled = true;
			}
		} else {
			OpenDoorPrompt.enabled = false;
		}
	}
}
