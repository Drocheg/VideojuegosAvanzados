using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbManager : MonoBehaviour {
	public float LimbDamage;
	public HealthManager HealthManager;

	public float TakeDamage(float DamageMultiplier)
	{
		return LimbDamage * DamageMultiplier;
	}
}
