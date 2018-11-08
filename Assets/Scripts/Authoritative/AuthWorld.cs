using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthWorld : MonoBehaviour {
	public float MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step, RotationStep, AnimationStep;

	public int MaxEntities;
	public float MaxTime, TimePrecision;
	private float _timestamp;
	public AuthNetworkManager NetworkManager;
	private AuthCharacterEntity[] entities;
	private int _expectedEntities;
	public int ExpectedEntities;
	private ParticlePool _sparksPool, _bloodPool;
	// Use this for initialization
	void Start () {
		entities = new AuthCharacterEntity[MaxEntities];
		_timestamp = 0;
		var pools = GetComponents<ParticlePool>();
		_sparksPool = pools[0];
		_bloodPool = pools[1];
	}
	
	// void Update() {
	// 	// Update entities positions
	// 	foreach(var e in entities) {
	// 		if (e != null) {
	// 			e.UpdateEntity(Time.deltaTime);
	// 		}
	// 	}
	// }


	// Update is called once per frame
	void FixedUpdate () {
		_timestamp += Time.fixedDeltaTime;
		if (_expectedEntities >= ExpectedEntities) {
			NetworkManager.SendAuthEventUnreliable(TakeSnapshot);
		}
	}

	public void AddReference(int id, AuthCharacterEntity auth)
	{
		_expectedEntities++;
		entities[id] = auth;
	}

	public void MovementCommand(int id, BitReader reader) {
		var entity = entities[id];
		if (entity != null) {
			var command = MoveCommand.Deserialize(reader, Step, RotationStep, MaxTime, TimePrecision);
			entity.Move(command);
		}
	}

	public void Shoot(int id, BitReader reader) {
		var comm = ShootCommand.Deserialize(reader, MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step);
		var commPos = new Vector3(comm._cX, comm._cY, comm._cZ);
		Debug.Log(comm._cX);
		ParticleSystem ps;

		if (comm._hitBlood) {
			ps = _sparksPool.GetParticleSystem();
			_sparksPool.ReleaseParticleSystem(ps);
		} else {
			ps = _bloodPool.GetParticleSystem();
			_bloodPool.ReleaseParticleSystem(ps);
		}
		ps.transform.position = commPos;
		ps.Play();
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
