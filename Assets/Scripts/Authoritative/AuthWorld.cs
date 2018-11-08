﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthWorld : MonoBehaviour {
	public float MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step, RotationStep, AnimationStep;

	public int MaxEntities;
	public float MaxTime, TimePrecision;
	public int MaxMoves;
	private float _timestamp;
	public AuthNetworkManager NetworkManager;
	private AuthCharacterEntity[] entities;
	private int _expectedEntities;
	public int ExpectedEntities;
	public float SnapshotTickRate;
	public float MaxHP, SpawnTime;
	public Transform SpawnLocation;
	private float _snapshotDelta; 
	private ParticlePool _sparksPool, _bloodPool;
	// Use this for initialization
	void Start () {
		entities = new AuthCharacterEntity[MaxEntities];
		_timestamp = 0;
		var pools = GetComponents<ParticlePool>();
		_sparksPool = pools[0];
		_bloodPool = pools[1];
		_snapshotDelta = 1 / SnapshotTickRate;
		StartCoroutine(SnapshotLoop());
	}
	
	// void Update() {
	// 	// Update entities positions
	// 	foreach(var e in entities) {
	// 		if (e != null) {
	// 			e.UpdateEntity(Time.deltaTime);
	// 		}
	// 	}
	// }
	IEnumerator SnapshotLoop() {
		while(true) {
			yield return new WaitForSecondsRealtime(_snapshotDelta);
			if (_expectedEntities >= ExpectedEntities) {
				NetworkManager.SendAuthEventUnreliable(TakeSnapshot);
			}
		}	
	}

	// Update is called once per frame
	void Update () {
		_timestamp += Time.deltaTime;
	}

	public void AddReference(int id, AuthCharacterEntity auth)
	{
		_expectedEntities++;
		entities[id] = auth;
	}

	public void MovementCommand(int id, BitReader reader) {
		var entity = entities[id];
		if (entity != null) {
			var command = MoveCommand.Deserialize(reader, Step, RotationStep, MaxTime, TimePrecision, MaxMoves);
			if (!entity.GetComponent<HealthManager>().Dead) {
				entity.Move(command);
			}
			
		}
	}

	public void Shoot(int id, BitReader reader) {
		var comm = ShootCommand.Deserialize(reader, MaxEntities, MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step);
		Shoot(id, comm);
	}

	IEnumerator ReviveEntity(HealthManager entity) {
		yield return new WaitForSeconds(SpawnTime);

		entity.SetHP(MaxHP);
		entity.transform.SetPositionAndRotation(SpawnLocation.position, SpawnLocation.rotation);

	}

	public void Revive(HealthManager entity) {
		StartCoroutine(ReviveEntity(entity));
	}

	public void Shoot(int id, ShootCommand comm) {
		var commPos = new Vector3(comm._cX, comm._cY, comm._cZ);
		var commNor = new Vector3(comm._nX, comm._nY, comm._nZ);
		ParticleSystem ps;

		if (comm._damage > 0) {
			ps = _bloodPool.GetParticleSystem();
			_bloodPool.ReleaseParticleSystem(ps);
			Debug.Log("Entity " + comm._id + " takes " + comm._damage + " damage.");
			var healthManager = entities[comm._id].GetComponent<HealthManager>();
			if (healthManager != null) {
				healthManager.TakeDamage(comm._damage);
				if (healthManager.Dead) {
					// entity was killed. Revive it.
					Revive(healthManager);
				}
			} else {
				Debug.Log("No health manager");
			}
		} else {
			ps = _sparksPool.GetParticleSystem();
			_sparksPool.ReleaseParticleSystem(ps);
		}
		ps.transform.SetPositionAndRotation(commPos, Quaternion.LookRotation(commNor));
		ps.Play();

		// Send shoot info to all hosts.
		var command = new ShootCommand(comm._damage, comm._id, MaxEntities, comm._cX, comm._cY, comm._cZ, comm._nX, comm._nY, comm._nZ, MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step);
		NetworkManager.SendAuthEventReliable(command.Serialize);
	}

	public void TakeSnapshot(BitWriter writer)
	{
		writer.WriteFloat(_timestamp, 0, MaxTime, TimePrecision);
		foreach(var entity in entities) {
			if (entity != null) {
				writer.WriteBit(true);
				entity.Serialize(writer);
			} else {
				writer.WriteBit(false);
			}
		}
	}
}
