using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalProjectileEntity : LocalEntity {
	public int Id;
	// Use this for initialization

	public Queue<Vector3> _qPositions;
	public Vector3? _pPos, _nPos;
	private LocalWorld _world;
	private NetworkState _currentState;
	private bool _started;
	void Awake () {
		_qPositions = new Queue<Vector3>();
		_world = GameObject.FindObjectOfType<LocalWorld>();
	}

	public override void UpdateEntity(float lerp) {
		if (_currentState == NetworkState.NORMAL) {
			LerpProjectile(lerp);
		}
	}

	public override bool NextInterval()
	{
		switch(_currentState){
			case NetworkState.INITIAL: {
				if (_qPositions.Count > _world.MinQueuedPositions) {
					_pPos = _qPositions.Dequeue();
					_nPos = _qPositions.Dequeue();
					_currentState = NetworkState.NORMAL;
					return true;
				}
				return false;
			}
			case NetworkState.NORMAL: {
				if (_qPositions.Count > 0) {
					_pPos = _nPos;
					_nPos = _qPositions.Dequeue();
					return true;
				}
				_pPos = null;
				_nPos = null;
				_currentState = NetworkState.INITIAL;
				return false;
			}
		}
		return false;
	}

	public void LerpProjectile(float lerp) {
		if (_pPos != null && _nPos != null) {
			transform.position = Vector3.Lerp(_pPos.Value, _nPos.Value, lerp);
		}
	}

	public override void Deserialize(BitReader reader) {
		Vector3 pos;
		pos.x = reader.ReadFloat(_world.MinPosX, _world.MaxPosX, _world.Step);
		pos.y = reader.ReadFloat(_world.MinPosY, _world.MaxPosY, _world.Step);
		pos.z = reader.ReadFloat(_world.MinPosZ, _world.MaxPosZ, _world.Step);

		_qPositions.Enqueue(pos);
	}

	public override int GetId() {
		return Id;
	}

	public override EntityType GetEntityType()
	{
		return EntityType.PROJECTILE;
	}
}
