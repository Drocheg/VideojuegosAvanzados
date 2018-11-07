using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthWorld : MonoBehaviour {
	public int MaxEntities;
	public float MaxTime, TimePrecision;
	private float _timestamp;
	public AuthNetworkManager NetworkManager;
	private IAuth[] entities;
	// Use this for initialization
	void Start () {
		entities = new IAuth[MaxEntities];
		_timestamp = 0;
	}
	

	// Update is called once per frame
	void FixedUpdate () {
		_timestamp += Time.fixedDeltaTime;
		NetworkManager.SendAuthEventUnreliable(TakeSnapshot);
	}

	public void AddReference(int id, IAuth auth)
	{
		entities[id] = auth;
	}

	public void TakeSnapshot(BitWriter writer)
	{
		writer.WriteFloat(_timestamp, 0, MaxTime, TimePrecision);
		foreach(var entity in entities) {
			if (entity != null) {
				writer.WriteBit(true);
				entity.Serialize(writer);
			} else {
				writer.WriteBit(false);
			}
		}
	}
}
