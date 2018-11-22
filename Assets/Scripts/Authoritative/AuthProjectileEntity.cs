using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthProjectileEntity : AuthEntity {
	public int Id, ShooterId;
	AuthWorld _authWorld;
	Rigidbody _rb;


	public void Awake() {
		_authWorld = GameObject.FindObjectOfType<AuthWorld>();
		_rb = GetComponent<Rigidbody>();
	}

	public override EntityType GetEntityType()
	{
		return EntityType.PROJECTILE;
	}

	public override int GetId(){
		return Id;
	}

	public override void Serialize(BitWriter writer) {
		writer.WriteFloat(transform.position.x, _authWorld.MinPosX, _authWorld.MaxPosX, _authWorld.Step);
		writer.WriteFloat(transform.position.y, _authWorld.MinPosY, _authWorld.MaxPosY, _authWorld.Step);
		writer.WriteFloat(transform.position.z, _authWorld.MinPosZ, _authWorld.MaxPosZ, _authWorld.Step);
		
	}

	public void SetPositionAndForce(Vector3 pos, Vector3 dir) {
		transform.position = pos;
		_rb.AddForce(dir * _authWorld.ExplosionMagnitude, ForceMode.Impulse);
	}
}
