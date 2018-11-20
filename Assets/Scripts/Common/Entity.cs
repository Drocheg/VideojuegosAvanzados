using UnityEngine;


public enum EntityType: int {
	CHARACTER = 0,
	PROJECTILE,
}
public abstract class Entity: MonoBehaviour {
	public abstract int GetId();
	public abstract EntityType GetEntityType();
}

public abstract class AuthEntity : Entity, IAuth
{
	public abstract void Serialize(BitWriter writer);
}

public abstract class LocalEntity : Entity, ILocal
{
	public abstract void Deserialize(BitReader reader);
	public abstract bool NextInterval();
	public abstract void UpdateEntity(float lerp);
}