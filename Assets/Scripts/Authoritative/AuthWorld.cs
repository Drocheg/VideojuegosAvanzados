using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthWorld : MonoBehaviour {
	public int MaxEntities;
	public float MaxTime, TimePrecision;
	private float _timestamp;
	public AuthNetworkManager NetworkManager;
	private AuthCharacterEntity[] entities;
	private int _expectedEntities;
	public int ExpectedEntities;
	public int MaxNumberOfPlayers;

	public AuthCharacterEntity e0;
	public AuthCharacterEntity e1;
	public AuthCharacterEntity e2;
	// Use this for initialization
	void Awake () {
		entities = new AuthCharacterEntity[MaxEntities];
		_timestamp = 0;
		entities[0] = e0;
		entities[1] = e1;
		entities[2] = e2;
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
		if (_expectedEntities >= ExpectedEntities) {
			NetworkManager.SendAuthEventUnreliable(TakeSnapshot);
		}
	}

	public void AddReference(int id, AuthCharacterEntity auth)
	{
		_expectedEntities++;
		entities[id] = auth;
	}

	public void MovementCommand(int id, BitReader reader) {
		var entity = entities[id];
		if (entity != null) {
			var command = MoveCommand.Deserialize(reader, entity.Step, entity.RotationStep, MaxTime, TimePrecision);
			entity.Move(command);
		}
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
