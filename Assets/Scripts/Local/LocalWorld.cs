using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	private float _timestamp;
	private NetworkState _currentState;
	private LocalCharacterEntity[] entities;

	int _entitiesCounter;
	public int ExpectedEntities;
	private LocalPlayer _localPlayer;
	// Use this for initialization
	void Start() {
		entities = new LocalCharacterEntity[MaxEntities];
		_queuedTimes = new Queue<float>();
		_entitiesCounter = 0;
		var pools = GetComponents<ParticlePool>();
		_sparksPool = pools[0];
		_bloodPool = pools[1];
		_localPlayer = GameObject.FindObjectOfType<LocalPlayer>();
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
					foreach(var e in entities) {
						if (e != null) {
							int lastProcessedInput;
							e.DequeNextPosition(out e._previousPosition, out e._previousAnimation, out e._previousRotation, out lastProcessedInput);
							e.DequeNextPosition(out e._nextPosition, out e._nextAnimation, out e._nextRotation, out lastProcessedInput);
							e.UpdateEntity(0);
						}
					}
					
					_currentState = NetworkState.NORMAL;
				}
				break;
			}
			case NetworkState.NORMAL: {
				var timeMultiplier = _queuedTimes.Count > TargetQueuedPositions ? 1.1f : 0.9f;
				// Debug.Log("TimeM: " + timeMultiplier);
				_currentTime += Time.deltaTime * timeMultiplier ;
				if (_currentTime > _nextTime) {
					if (_queuedTimes.Count > 0) {
						_previousTime = _nextTime;
						_nextTime = _queuedTimes.Dequeue();
						foreach(var e in entities) {
							if (e != null) {
								e._previousPosition = e._nextPosition;
								e._previousAnimation = e._nextAnimation;
								e._previousRotation = e._nextRotation;
								int lastProcessedInput;
								e.DequeNextPosition(out e._nextPosition, out e._nextAnimation, out e._nextRotation, out lastProcessedInput);
								if (e.IsLocalPlayer) {
									_localPlayer.AdjustPositionFromSnapshot(e._nextPosition.Value, lastProcessedInput);
								}
							}
						}
						
						if (_nextTime - _currentTime > MaxAllowedDelay) {
							// Hard reset
							_currentState = NetworkState.INITIAL;
							Debug.Log("Hard reset");
							break;
						}
					} else {
						foreach(var e in entities) {
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
				foreach(var e in entities) {
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

	public void AddReference(int id, LocalCharacterEntity local)
	{
		_entitiesCounter++;
		entities[id] = local;
	}

	public void RemoveReference(int id)
	{
		entities[id] = null;
		_entitiesCounter--;
	}


	public void NewSnapshot(BitReader reader) {
		if (_entitiesCounter < ExpectedEntities) {
			return;
		}
		QueueNextSnapshot(reader.ReadFloat(0, MaxTime, TimePrecision));
		
		foreach(var e in entities) {
			var b = reader.ReadBit();
			if (b) {
				Debug.Log("Snapshot para id: " + e.Id);
				Debug.Assert(e != null);
				e.Deserialize(reader);
			} 
		}
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
		}
		ps.transform.SetPositionAndRotation(commPos, Quaternion.LookRotation(commNor));
		ps.Play();
	}
}
