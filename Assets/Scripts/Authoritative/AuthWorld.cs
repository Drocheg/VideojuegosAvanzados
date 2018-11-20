using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AuthWorld : MonoBehaviour {
	public float MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step, RotationStep, AnimationStep;
	public AuthProjectileEntity AuthProjectile;
	public int MaxEntities, MaxProjectiles;
	public float MaxTime, TimePrecision;
	public ulong MaxMoves;
	private float _timestamp;
	private AuthNetworkManager _networkManager;
	private AuthEntity[] _entities;
	private int _expectedEntities;
	public int ExpectedEntities;


	//public AuthCharacterEntity e0;
	//public AuthCharacterEntity e1;
	//public AuthCharacterEntity e2;
	
	public float SnapshotTickRate;
	public float MaxHP, SpawnTime;
	public Transform SpawnLocation;
	private float _snapshotDelta; 
	private ParticlePool _sparksPool, _bloodPool;
	public int ProjectileOffset;
	private int _entityTypes;
	public float ExplosionMagnitude;
	 
	// Use this for initialization
	void Awake () {
		_entities = new AuthEntity[MaxEntities];
		_timestamp = 0; 
	//	entities[0] = e0;
	//	entities[1] = e1;
	//	entities[2] = e2;
		var pools = GetComponents<ParticlePool>();
		_sparksPool = pools[0];
		_bloodPool = pools[1];
		_snapshotDelta = 1 / SnapshotTickRate;
		StartCoroutine(SnapshotLoop());
	}
	
	void Start()
	{
		_entityTypes = System.Enum.GetValues(typeof (EntityType)).Length;
		_networkManager = GameObject.FindObjectOfType<AuthNetworkManager>();
	}

	IEnumerator SnapshotLoop() {
		while(true) {
			yield return new WaitForSecondsRealtime(_snapshotDelta);
			if (_expectedEntities >= ExpectedEntities) {
				_networkManager.SendAuthEventUnreliable(TakeSnapshot);
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
		_entities[id] = auth;
	}

	public void MovementCommand(int id, BitReader reader) {
		if (id < 0 || id >= _entities.Length) {
			Debug.LogWarning("Character id is not within array boundaries");
			return;
		}
		var entity = _entities[id];
		if (entity != null && entity.GetEntityType() == EntityType.CHARACTER) {
			var authEntity = entity.GetComponent<AuthCharacterEntity>();
			var command = MoveCommand.Deserialize(reader, Step, RotationStep, MaxTime, TimePrecision, MaxMoves);
			if (!authEntity.GetComponent<HealthManager>().Dead) {
				authEntity.Move(command);
			}
		} else {
			Debug.LogWarning("Tried to move something unmovable (Wrong id?).");
		}
	}


	IEnumerator ReviveEntity(HealthManager entity) {
		yield return new WaitForSeconds(SpawnTime);

		entity.SetHP(MaxHP);
		entity.transform.SetPositionAndRotation(SpawnLocation.position, SpawnLocation.rotation);

	}

	public void Revive(HealthManager entity) {
		StartCoroutine(ReviveEntity(entity));
	}
	public void Shoot(int id, BitReader reader) {
		var comm = ShootCommand.Deserialize(reader, MaxEntities, MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step);
		Shoot(id, comm);
	}

	public void Shoot(int id, ShootCommand comm) {
		var commPos = new Vector3(comm._cX, comm._cY, comm._cZ);
		var commNor = new Vector3(comm._nX, comm._nY, comm._nZ);
		ParticleSystem ps;

		if (comm._damage > 0) {
			ps = _bloodPool.GetParticleSystem();
			_bloodPool.ReleaseParticleSystem(ps);
			Debug.Log("Entity " + comm._id + " takes " + comm._damage + " damage.");
			var healthManager = _entities[comm._id].GetComponent<HealthManager>();
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
		_networkManager.SendAuthEventReliable(command.Serialize);
	}

	public void Explode(AuthProjectileEntity projectile, WithinExplosionRadius radius, float damage) {
		radius.Explode(damage, this);
		// send explosion.
		int id = projectile.GetId();
		if (id < 0 || id >= _entities.Length) {
			Debug.LogWarning("Projectile id out of boundaries.");
			return;
		}
		_entities[projectile.GetId()] = null;
	}

	public void NewProjectile(ProjectileShootCommand command) {
		for(int i = 0; i < _entities.Length; i++) {
			if (_entities[i] == null) {
				var projectile = Instantiate(AuthProjectile);
				projectile.Id = i;
				var pos = new Vector3(command._x, command._y, command._z);
				var dir = new Vector3(command._dirX, command._dirY, command._dirZ);
				projectile.SetPositionAndForce(pos, dir);
				_entities[i] = projectile;

				command._id = i;
				_networkManager.SendAuthEventReliable(command.Serialize);
				break;
			}
		}
	}

	public void TakeSnapshot(BitWriter writer)
	{
		writer.WriteFloat(_timestamp, 0, MaxTime, TimePrecision);
		foreach(var entity in _entities) {
			if (entity != null) {
				Debug.Log("Creando snapshot para id: " + entity.GetId());
				writer.WriteBit(true);
				writer.WriteInt((ulong) entity.GetEntityType(), 0, (uint) _entityTypes);
				entity.Serialize(writer);
			} else {
				writer.WriteBit(false);
			}
		}
	}

}
