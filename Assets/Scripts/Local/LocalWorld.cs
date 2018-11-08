using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalWorld : MonoBehaviour {
	public int MaxEntities;
	public int MaxNumberOfPlayer;
	public float MaxTime, TimePrecision, MaxAllowedDelay;
	private Queue<float> _queuedTimes;
	public int MinQueuedPositions, MaxQueuedPositions, TargetQueuedPositions;
	private float _previousTime, _nextTime, _currentTime;
	private float _timestamp;
	private NetworkState _currentState;
	private LocalCharacterEntity[] entities;

	public enum NetworkState {
		INITIAL,
		NORMAL,
		NETWORK_PROBLEMS,
	}
	int _entitiesCounter;
	public int ExpectedEntities;
	// Use this for initialization
	void Start () {
		entities = new LocalCharacterEntity[MaxEntities];
		_queuedTimes = new Queue<float>();
		_entitiesCounter = 0;
	}
	
	// Update is called once per frame
	void Update () {
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
							e.DequeNextPosition(out e._previousPosition, out e._previousAnimation, out e._previousRotation);
							e.DequeNextPosition(out e._nextPosition, out e._nextAnimation, out e._nextRotation);
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
								e.DequeNextPosition(out e._nextPosition, out e._nextAnimation, out e._nextRotation);
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
		
		int c = 0;
		foreach(var e in entities) {
			var b = reader.ReadBit();
			if (b) {
				c++;
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
}
