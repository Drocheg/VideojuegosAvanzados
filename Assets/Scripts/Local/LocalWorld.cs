using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalWorld : MonoBehaviour {
	public int MaxEntities;
	public float MaxTime, TimePrecision;
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

	// Use this for initialization
	void Start () {
		entities = new LocalCharacterEntity[MaxEntities];
		_queuedTimes = new Queue<float>();
	}
	
	// Update is called once per frame
	void Update () {
		// Read packets from NetworkAPI

		switch(_currentState) {
			case NetworkState.INITIAL: {
				// Initial position arrived but not enough info to interpolate.
				if (_queuedTimes.Count >= MinQueuedPositions) {
					Debug.Assert(_queuedTimes.Count >= 2);
					_previousTime = _queuedTimes.Dequeue();
					_nextTime = _queuedTimes.Dequeue();
					foreach(var e in entities) {
						if (e != null) {
							e.DequeNextPosition(out e._previousPosition, out e._previousAnimation, out e._previousRotation);
							e.DequeNextPosition(out e._nextPosition, out e._nextAnimation, out e._nextRotation);
							e.UpdateEntity(0);
						}
					}
				}
				break;
			}
			case NetworkState.NORMAL: {
				var timeMultiplier = _queuedTimes.Count > TargetQueuedPositions ? 1.1f : 1;
				// Debug.Log("TimeM: " + timeMultiplier);
				Debug.Log("Q: " + _queuedTimes.Count);
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
					} else {
						foreach(var e in entities) {
							if (e != null ) e.UpdateEntity(1);
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
		entities[id] = local;
	}

	public void NewSnapshot(BitReader reader) {
		_queuedTimes.Enqueue( reader.ReadFloat(0,MaxTime, TimePrecision));
		foreach(var e in entities) {
			if (reader.ReadBit()) {
				Debug.Assert(e != null);
				e.Deserialize(reader);
			} 
		}
	}

	
}
