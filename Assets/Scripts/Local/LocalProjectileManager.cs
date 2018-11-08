using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalProjectileManager : MonoBehaviour {
	
	public LocalProjectileEntity LocalProjectile;

	private LocalProjectileEntity[] projectiles;
	private NetworkState _currentState;

	LocalWorld _world;
	// Use this for initialization
	void Start () {
		_world = GameObject.FindObjectOfType<LocalWorld>();
	}
	
	// Update is called once per frame
	void Update () {
		foreach(var e in projectiles) {
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
				if (projectiles[i] == null) {
					// New projectile.
					projectiles[i] = Instantiate(LocalProjectile);
				}
				// Existing projectile
				projectiles[i].Deserialize(reader);
				projectiles[i]._queuedTimes.Enqueue(t);
				projectiles[i].Deserialize(reader);
			} else if (projectiles[i] != null) {
				// Projectile exploded
				projectiles[i].GetComponent<Projectile>().Explode();
				projectiles[i] = null;
			}
		}
	}
}
