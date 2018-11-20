using UnityEngine;


public enum EntityType: int {
	CHARACTER = 0,
	PROJECTILE,
}
public abstract class Entity: MonoBehaviour {
	public abstract int GetId();
	public abstract EntityType GetEntityType();
	public static int[] EntitySizes = new int[System.Enum.GetValues(typeof(EntityType)).Length];

	public static int GetSerialBits(int type) {
		if (type < 0 || type >= EntitySizes.Length) {
			return 0;
		}
		return EntitySizes[type];
	}
}

public abstract class AuthEntity : Entity, IAuth
{
	public abstract void Serialize(BitWriter writer);
}

public abstract class LocalEntity : Entity, ILocal
{
	public abstract void Deserialize(BitReader reader);
}