using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthWorld : MonoBehaviour {
	public int MaxEntities;
	public float MaxTime, TimePrecision;
	private float _timestamp;
	public AuthNetworkManager NetworkManager;
	private AuthCharacterEntity[] entities;
	// Use this for initialization
	void Start () {
		entities = new AuthCharacterEntity[MaxEntities];
		_timestamp = 0;
	}
	
	// void Update() {
	// 	// Update entities positions
	// 	foreach(var e in entities) {
	// 		if (e != null) {
	// 			e.UpdateEntity(Time.deltaTime);
	// 		}
	// 	}
	// }


	// Update is called once per frame
	void FixedUpdate () {
		_timestamp += Time.fixedDeltaTime;
		NetworkManager.SendAuthEventUnreliable(TakeSnapshot);
	}

	public void AddReference(int id, AuthCharacterEntity auth)
	{
		entities[id] = auth;
	}

	public void MovementCommand(int id, BitReader reader) {
		var entity = entities[id];
		var command = MoveCommand.Deserialize(reader, entity.Step, MaxTime, TimePrecision);
		entity.Move(command);
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
