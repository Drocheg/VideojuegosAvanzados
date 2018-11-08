using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalProjectileEntity : CharacterEntity {
	public int Id;
	// Use this for initialization

	public Queue<Vector3> _qPositions;
	public Vector3? _pPos, _nPos;
	private LocalWorld _world;

	private float _previousTime, _nextTime, _currentTime;
	public Queue<float> _queuedTimes;
	private NetworkState _currentState;
	void Awake () {
		_qPositions = new Queue<Vector3>();
		_world = GameObject.FindObjectOfType<LocalWorld>();
	}
	
	// Update is called once per frame
	public void UpdateProjectile () {
		switch(_currentState) {
			case NetworkState.INITIAL: {
				// Initial position arrived but not enough info to interpolate.
				if (_queuedTimes.Count >= _world.MinQueuedPositions) {
					Debug.Assert(_queuedTimes.Count >= 2);
					_previousTime = _queuedTimes.Dequeue();
					_nextTime = _queuedTimes.Dequeue();
					_pPos = _qPositions.Dequeue();
					_nPos = _qPositions.Dequeue();
					_currentTime = _previousTime;
					_currentState = NetworkState.NORMAL;
				}
				break;
			}
			case NetworkState.NORMAL: {
				var timeMultiplier = _queuedTimes.Count > _world.TargetQueuedPositions ? 1.1f : 0.9f;
				// Debug.Log("TimeM: " + timeMultiplier);
				_currentTime += Time.deltaTime * timeMultiplier ;
				if (_currentTime > _nextTime) {
					if (_queuedTimes.Count > 0) {
						_previousTime = _nextTime;
						_nextTime = _queuedTimes.Dequeue();
						_pPos = _nPos;
						_nPos = _qPositions.Dequeue();
						
						if (_nextTime - _currentTime > _world.MaxAllowedDelay) {
							// Hard reset
							_currentState = NetworkState.INITIAL;
							Debug.Log("Hard reset");
							break;
						}
					} else {
						LerpProjectile(1);
						_currentState = NetworkState.INITIAL;
						Debug.Log("Network problems");
						break;
					}
				}
				var d = (_currentTime - _previousTime) / (_nextTime - _previousTime);
				LerpProjectile(d);
				break;
			}
			case NetworkState.NETWORK_PROBLEMS: {
				break;
			}
		}
	}
	
	public void LerpProjectile(float lerp) {
		transform.position = Vector3.Lerp(_pPos.Value, _nPos.Value, lerp);
	}

	public void Deserialize(BitReader reader) {
		Vector3 pos;
		pos.x = reader.ReadFloat(_world.MinPosX, _world.MaxPosX, _world.Step);
		pos.y = reader.ReadFloat(_world.MinPosY, _world.MaxPosY, _world.Step);
		pos.z = reader.ReadFloat(_world.MinPosZ, _world.MaxPosZ, _world.Step);

		_qPositions.Enqueue(pos);
	}

	public override int GetId() {
		return Id;
	}
}
