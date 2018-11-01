using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickUpItem : MonoBehaviour {
	public string Name;
	public abstract void PickUp(PickUpManager pickUpManager);
}
