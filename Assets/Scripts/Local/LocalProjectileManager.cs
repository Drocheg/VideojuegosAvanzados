using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalProjectileManager : MonoBehaviour {
	
	public LocalProjectileEntity LocalProjectile;

	private LocalProjectileEntity[] _projectiles;
	private NetworkState _currentState;

	LocalWorld _world;
	// Use this for initialization
	void Start () {
		_world = GameObject.FindObjectOfType<LocalWorld>();
		_projectiles = new LocalProjectileEntity[_world.MaxProjectiles];
	}
	
	// Update is called once per frame
	void Update () {
		foreach(var e in _projectiles) {
			if (e != null) {
				e.UpdateProjectile();
			}
		}
	}

	public void NewSnapshot(BitReader reader) {

		var t = reader.ReadFloat(0, _world.MaxTime, _world.TimePrecision);
		for (int i = 0; i < _world.MaxProjectiles; i++) {
			bool b = reader.ReadBit();
			if (b) {
				Debug.Log("Snapshots");
				if (_projectiles[i] == null) {
					// New projectile.
					_projectiles[i] = Instantiate(LocalProjectile);
				}
				// Existing projectile
				_projectiles[i].Deserialize(reader);
				_projectiles[i]._queuedTimes.Enqueue(t);
			} else if (_projectiles[i] != null) {
				// Projectile exploded
				_projectiles[i].GetComponent<Projectile>().Explode();
				_projectiles[i] = null;
			}
		}
	}
}
