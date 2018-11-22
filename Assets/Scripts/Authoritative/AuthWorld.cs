using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerGameState {
	public uint id, kills, deaths;
}

public class AuthWorld : MonoBehaviour {
	public float MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step, RotationStep, AnimationStep;
	public AuthProjectileEntity AuthProjectile;
	public int MaxEntities, MaxProjectiles;
	public uint MaxKills, MaxDeaths;
	public float MaxTime, TimePrecision, GameStateDelta;
	public ulong MaxMoves;
	private float _timestamp;
	private AuthNetworkManager _networkManager;
	private AuthEntity[] _entities;
	private int _expectedEntities;
	public int ExpectedEntities;
	public float SnapshotTickRate;
	public float MaxHP, SpawnTime;
	public Transform SpawnLocation;
	private float _snapshotDelta; 
	private ParticlePool _sparksPool, _bloodPool;
	public int ProjectileOffset;
	private int _entityTypes;
	public float ExplosionMagnitude;
	private uint _maxPlayers;
	private List<PlayerGameState> _playerStates;
	public TextMeshProUGUI GameStateText, TimerGUI;

	// Use this for initialization
	void Awake () {
		_entities = new AuthEntity[MaxEntities];
		_timestamp = 0; 
		var pools = GetComponents<ParticlePool>();
		_sparksPool = pools[0];
		_bloodPool = pools[1];
		_snapshotDelta = 1 / SnapshotTickRate;
		StartCoroutine(SnapshotLoop());
		StartCoroutine(GameStateLoop());
	}
	
	void Start()
	{
		_entityTypes = System.Enum.GetValues(typeof (EntityType)).Length;
		_networkManager = GameObject.FindObjectOfType<AuthNetworkManager>();
		_maxPlayers = _networkManager.MaxHosts + 1;
		_playerStates = new List<PlayerGameState>();
		AddPlayer(0); // Add host as player.
	}

	public void AddPlayer(uint id) {
		_playerStates.Add(new PlayerGameState(){
			id = id,
			kills = 0,
			deaths = 0,
		});
	}

	public void RemovePlayer(uint id) {
		var playerState = _playerStates.Find(x => x.id == id);
		if (playerState != null) {
			_playerStates.Remove(playerState);
		} else {
			Debug.LogWarning("Tried to remove an unexistent player");
		}
	}

	IEnumerator SnapshotLoop() {
		while(true) {
			yield return new WaitForSecondsRealtime(_snapshotDelta);
			if (_expectedEntities >= ExpectedEntities) {
				_networkManager.SendAuthSnapshotUnreliable(TakeSnapshot);
			}
		}	
	}

	void CreateArraysFromGameStateList(List<PlayerGameState> list, out uint[] ids, out uint[] kills, out uint[] deaths) {
		ids = new uint[list.Count];
		kills = new uint[list.Count];
		deaths = new uint[list.Count];
		for(int i = 0; i < list.Count; i++) {
			ids[i] = list[i].id;
			kills[i] = list[i].kills;
			deaths[i] = list[i].deaths;
		}
	}

	IEnumerator GameStateLoop() {
		while(true) {
			yield return new WaitForSecondsRealtime(GameStateDelta);
			uint[] ids, kills, deaths;
			CreateArraysFromGameStateList(_playerStates, out ids, out kills, out deaths);
			var comm = new GameState() {
				Deaths = deaths,
				Ids = ids,
				Kills = kills,
				MaxDeaths = MaxDeaths,
				MaxKills = MaxKills,
				MaxEntities = (uint) MaxEntities,
				MaxTotalPlayers = _maxPlayers,
				TotalPlayers = (uint) _playerStates.Count
			};
			_networkManager.SendAuthEventReliable(comm.Serialize);
		}	
	}

	// Update is called once per frame
	void Update () {
		_timestamp += Time.deltaTime;

		int totalSeconds = (int)_timestamp;
		int seconds = totalSeconds % 60;
		int minutes = totalSeconds / 60;
		string time = minutes + ":" + seconds;
		TimerGUI.text = time;


		var state = "";
		if (Input.GetButtonDown("GameState")) {
			foreach(var playerState in _playerStates) {
				state += string.Format("Player {0}\t{1}K\t{2}D\n", playerState.id, playerState.kills, playerState.deaths);
			}
			GameStateText.text = state;
		}
		if (Input.GetButtonUp("GameState")) {
			GameStateText.text = "";
		}
	}

	public void AddReference(int id, AuthCharacterEntity auth)
	{
		_expectedEntities++;
		_entities[id] = auth;
	}
	
	public void RemoveEntity(uint id)
	{
		AuthEntity removedEntity = _entities[id];
		if (removedEntity != null)
		{
			Destroy(removedEntity.gameObject);
			RemoveReference(id);
		}
	}
	
	public void RemoveReference(uint id)
	{
		_entities[id] = null;
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

		if (entity != null)
		{
			entity.SetHP(MaxHP);
			entity.transform.SetPositionAndRotation(SpawnLocation.position, SpawnLocation.rotation);	
		}
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
					AccountPlayerKill(id, comm._id);
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
		Debug.Log("Erasing projectile " + projectile.GetId());
		_entities[projectile.GetId()] = null;
		// send Explosion command.
		var command = new ProjectileExplodeCommand() {
			pos = projectile.transform.position,
			nor = projectile.transform.up,
			minPos = new Vector3(MinPosX, MinPosY, MinPosZ),
			maxPos = new Vector3(MaxPosX, MaxPosY, MaxPosZ),
			minDir = new Vector3(MinPosX, MinPosY, MinPosZ),
			maxDir = new Vector3(MaxPosX, MaxPosY, MaxPosZ),
			id = id,
			maxId = MaxEntities,
			directionPrecision = Step,
			positionPrecision = Step,
		};
		_networkManager.SendAuthEventReliable(command.Serialize);
	}

	public void NewProjectile(int shooterId, BitReader reader) {
		var command = ProjectileShootCommand.Deserialize(
			reader,
			MaxEntities,
			MinPosX,
			MaxPosX,
			MinPosY,
			MaxPosY,
			MinPosZ,
			MaxPosZ,
			Step
		);

		NewProjectile(shooterId, new Vector3(
			command._x,
			command._y,
			command._z
		), new Vector3(
			command._dirX,
			command._dirY,
			command._dirZ
		));
	}

	public void NewProjectile(int shooterId, Vector3 pos, Vector3 dir) {
		var command = new ProjectileShootCommand(
				0, 
				MaxEntities,
				pos.x,
				pos.y,
				pos.z,
				dir.x,
				dir.y,
				dir.z,
				MinPosX,
				MaxPosX,
				MinPosY,
				MaxPosY,
				MinPosZ,
				MaxPosZ, 
				Step);
		for(int i = 0; i < _entities.Length; i++) {
			if (_entities[i] == null) {
				var projectile = Instantiate(AuthProjectile);
				projectile.Id = i;
				projectile.ShooterId = shooterId;
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
				writer.WriteBit(true);
				writer.WriteInt((ulong) entity.GetEntityType(), 0, (uint) _entityTypes);
				entity.Serialize(writer);
			} else {
				writer.WriteBit(false);
			}
		}
	}

	public void AccountPlayerKill(int killerId, int victimId) {
		var killer = _playerStates.Find((x) => x.id == killerId);
		if (killer != null) {
			killer.kills++;
			
		}
		var victim = _playerStates.Find((x) => x.id == victimId);
		if (victim != null) {
			victim.deaths++;
		}
	}

}
