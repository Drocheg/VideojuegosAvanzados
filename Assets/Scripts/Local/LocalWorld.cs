﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum NetworkState {
	INITIAL,
	NORMAL,
	NETWORK_PROBLEMS,
}

public class LocalWorld : MonoBehaviour {
	public int MaxEntities, MaxProjectiles;
	public float MaxTime, TimePrecision, MaxAllowedDelay;
	public float MaxHP;
	public float MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step, RotationStep, AnimationStep;
	private ParticlePool _sparksPool, _bloodPool;
	private Queue<float> _queuedTimes;
	public int MinQueuedPositions, MaxQueuedPositions, TargetQueuedPositions;
	private float _previousTime, _nextTime, _currentTime;
	private NetworkState _currentState;
	private LocalEntity[] _entities;
	private string[] _playerNames;

	int _entitiesCounter, _entityTypes;
	public int ExpectedEntities;
	private LocalPlayer _localPlayer;
	public LocalProjectileEntity LocalProjectilePrefab;
	private int _characterSerialSize, _projectileSerialSize;
	private LocalNetworkManager _networkManager;
	public uint MaxDeaths, MaxKills;
	public TextMeshProUGUI GameStateGUI, TimerGUI;
	SpritePool _bulletPool;
	string _gameStateString = "";
	// Use this for initialization
	void Start() {
		_entities = new LocalEntity[MaxEntities];
		_queuedTimes = new Queue<float>();
		_entitiesCounter = 0;
		var pools = GetComponents<ParticlePool>();
		_sparksPool = pools[0];
		_bloodPool = pools[1];
		_localPlayer = GameObject.FindObjectOfType<LocalPlayer>();
		_entityTypes = System.Enum.GetValues(typeof(EntityType)).Length;
		_characterSerialSize = CharacterEntitySerialSize();
		_projectileSerialSize = ProjectileEntitySerialSize();
		_networkManager = FindObjectOfType<LocalNetworkManager>();
		_bulletPool = FindObjectOfType<SpritePool>();
		_playerNames = new string[MaxEntities];

	}

	void Update() {
		int totalSeconds = (int) _currentTime;
		int seconds = totalSeconds % 60;
		int minutes = totalSeconds / 60;
		string time = minutes + ":" + seconds;
		TimerGUI.text = time;

		if (Input.GetButtonDown("GameState")) {
			GameStateGUI.text = _gameStateString;
		}
		if (Input.GetButtonUp("GameState")) {
			GameStateGUI.text = "";
		}
	}

	public void AddPlayerName(int playerId, string playerName)
	{
		if (playerId < MaxEntities) _playerNames[playerId] = playerName;
	}

	// Update is called once per frame
	void LateUpdate () {
		// Read packets from NetworkAPI
		if (_entitiesCounter < ExpectedEntities) {
			return;
		}
		switch(_currentState) {
			case NetworkState.INITIAL: {
				// Initial position arrived but not enough info to interpolate.
				if (_queuedTimes.Count >= MinQueuedPositions) {
					Debug.Assert(_queuedTimes.Count >= 2);
					_previousTime = _queuedTimes.Dequeue();
					_nextTime = _queuedTimes.Dequeue();
					_currentTime = _previousTime;
					foreach(var e in _entities) {
						if (e != null) {
							// Interpol entity.
							e.NextInterval();
							e.UpdateEntity(0);
						}
					}

					_currentState = NetworkState.NORMAL;
				}
				break;
			}
			case NetworkState.NORMAL: {
				var timeMultiplier = _queuedTimes.Count > TargetQueuedPositions ? 1.1f : 1f;
				// Debug.Log("TimeM: " + timeMultiplier);
				Debug.Log("time multiplier " + timeMultiplier);
				_currentTime += Time.deltaTime * timeMultiplier ;
				if (_currentTime > _nextTime) {
					if (_queuedTimes.Count > 0) {
						_previousTime = _nextTime;
						_nextTime = _queuedTimes.Dequeue();
						foreach(var e in _entities) {
							// Interpol entity
							if (e != null) {
								e.NextInterval();
							}
						}

						if (_nextTime - _currentTime > MaxAllowedDelay) {
							// Hard reset
							_currentState = NetworkState.INITIAL;
							Debug.Log("Hard reset");
							break;
						}
					} else {
						foreach(var e in _entities) {
							if (e != null ) {
								e.UpdateEntity(1);
							}
						}
						_currentState = NetworkState.INITIAL;
						Debug.Log("Network problems");
						break;
					}
				}
				var d = (_currentTime - _previousTime) / (_nextTime - _previousTime);
				foreach(var e in _entities) {
					if (e != null) {
						e.UpdateEntity(d);
					}
				}
				break;
			}
			case NetworkState.NETWORK_PROBLEMS: {
				break;
			}
		}
	}

	public LocalEntity GetCharacterEntity(uint id)
	{
		return _entities[id];
	}

	public void AddReference(int id, LocalCharacterEntity local)
	{
		_entitiesCounter++;
		_entities[id] = local;
	}

	public void RemoveEntity(uint id)
	{
		if (id < MaxEntities) _playerNames[id] = null;
		LocalEntity removedEntity = _entities[id];
		if (removedEntity != null)
		{
			Destroy(removedEntity.gameObject);
			RemoveReference(id);
		}
	}

	public void RemoveReference(uint id)
	{
		_entities[id] = null;
		_entitiesCounter--;
	}


	public void NewSnapshot(BitReader reader) {
		if (_entitiesCounter < ExpectedEntities) {
			return;
		}
		QueueNextSnapshot(reader.ReadFloat(0, MaxTime, TimePrecision));

		foreach(var e in _entities) {
			var b = reader.ReadBit();
			if (b) {
				int entityType = reader.ReadInt(0, _entityTypes);
				if (e == null) {
					Debug.Log("Update received for unknown entity");
					var bitsToDiscard = 0;
					switch(entityType) {
						case (int)EntityType.CHARACTER: {
							bitsToDiscard = _characterSerialSize;
							Debug.Log("Discarding character bits: " + bitsToDiscard);
							break;
						}
						case (int)EntityType.PROJECTILE: {
							bitsToDiscard = _projectileSerialSize;
							Debug.Log("Discarding projectile bits: " + bitsToDiscard);
							break;
						}
						default:
							Debug.Log("Unknown entity, discarding 0 bits");
							break;
					}
					reader.DiscardBits(bitsToDiscard);
				} else {
					Debug.Assert(e != null);
					e.Deserialize(reader);
				}
			}
		}
	}

	public void NewProjectileShootCommand(BitReader reader) {
		var command = ProjectileShootCommand.Deserialize(
			reader,
			MaxEntities,
			MinPosX,
			MinPosY,
			MinPosZ,
			MaxPosX,
			MaxPosY,
			MaxPosZ,
			Step
		);

		if (command._id < 0 || command._id >= MaxEntities) {
			Debug.LogWarning("Recevied projectile shoot command with invalid id.");
			return;
		}

		var projectile = Instantiate(LocalProjectilePrefab);
		projectile.Id = command._id;
		_entities[projectile.Id] = projectile;
		var pos = new Vector3(command._x, command._y, command._z);
		projectile.transform.position = pos;
	}

	public void ProjectileExplosion(BitReader reader) {
		var command = ProjectileExplodeCommand.Deserialize(
			reader,
			MaxEntities,
			Step,
			Step,
			new Vector3(MinPosX, MinPosY, MinPosZ),
			new Vector3(MaxPosX, MaxPosY, MaxPosZ),
			new Vector3(MinPosX, MinPosY, MinPosZ),
			new Vector3(MaxPosX, MaxPosY, MaxPosZ)
		);
		Debug.Log("Command " + command);
		if (command.id < 0 || command.id >= MaxEntities) {
			Debug.LogWarning("Received explosion with invalid id.");
			return;
		}
		var projectile = _entities[command.id];
		var p = projectile.GetComponent<Projectile>();
		if (p == null) {
			Debug.LogWarning("Tried to explode but there was no projectile.");
		} else {
			_entities[command.id] = null;
			p.Explode();
		}
	}

	int CharacterEntitySerialSize() {
		int count = 0;
		count += Utility.CountBitsFloat(MinPosX, MaxPosX, Step);
		count += Utility.CountBitsFloat(MinPosY, MaxPosY, Step);
		count += Utility.CountBitsFloat(MinPosZ, MaxPosZ, Step);
		count += Utility.CountBitsFloat(-1, 1, AnimationStep);
		count += Utility.CountBitsFloat(-1, 1, AnimationStep);
		count += Utility.CountBitsFloat(-1, 360, RotationStep);
		count += Utility.CountBitsInt(0, (int)_localPlayer.MaxMoves);
		count += Utility.CountBitsFloat(0, MaxHP, 0.1f);
		return count;
	}

	int ProjectileEntitySerialSize() {
		int count = 0;
		count += Utility.CountBitsFloat(MinPosX, MaxPosX, Step);
		count += Utility.CountBitsFloat(MinPosY, MaxPosY, Step);
		count += Utility.CountBitsFloat(MinPosZ, MaxPosZ, Step);
		return count;
	}

	void QueueNextSnapshot(float timestamp) {
		while (_queuedTimes.Count >= MaxQueuedPositions) {
			_queuedTimes.Dequeue();
		}
		_queuedTimes.Enqueue(timestamp);
	}

	public void BulletCollision(BitReader reader) {
		var comm = ShootCommand.Deserialize(reader, MaxEntities, MinPosX, MaxPosX, MinPosY, MaxPosY, MinPosZ, MaxPosZ, Step );

		var commPos = new Vector3(comm._cX, comm._cY, comm._cZ);
		var commNor = new Vector3(comm._nX, comm._nY, comm._nZ);
		ParticleSystem ps;

		if (comm._damage > 0) {
			ps = _bloodPool.GetParticleSystem();
			_bloodPool.ReleaseParticleSystem(ps);
		} else {
			ps = _sparksPool.GetParticleSystem();
			_sparksPool.ReleaseParticleSystem(ps);

			var bullet = _bulletPool.GetSprite();
			bullet.transform.SetPositionAndRotation(commPos + commNor * 0.001f, Quaternion.LookRotation(commNor));
		}
		ps.transform.SetPositionAndRotation(commPos, Quaternion.LookRotation(commNor));
		ps.Play();
	}

	public void ShootProjectile(Vector3 pos, Vector3 dir) {
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

		_networkManager.SendReliable(command.Serialize);
	}

	public void UpdateGameState(BitReader reader) {
		var state = GameState.Deserialize(
			reader,
			_networkManager.MaxPlayers,
			MaxDeaths,
			MaxKills,
			(uint) MaxEntities
		);
		_gameStateString = "";
		for(int i = 0; i < state.TotalPlayers; i++)
		{
			uint playerId = state.Ids[i];
			if (string.IsNullOrEmpty(_playerNames[playerId]))
			{
				_gameStateString += string.Format(
					"Player {0}\t{1}K\t{2}D\n",
					state.Ids[i],
					state.Kills[i],
					state.Deaths[i]
				);
			}
			else
			{
				_gameStateString += string.Format(
					"{0}\t{1}K\t{2}D\n",
					_playerNames[playerId],
					state.Kills[i],
					state.Deaths[i]
				);
			}
			
		}
	}
}
