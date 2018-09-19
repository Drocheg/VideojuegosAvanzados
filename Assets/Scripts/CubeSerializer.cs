using UnityEngine;

public class CubeSerializer : MonoBehaviour, ISerial
{
    public float min, max, step;
    private Vector3 PositionCopy;

    public void Start()
    {
        SnapshotSerializer.GetInstance().AddReference(0, this);
    }

    public void Update() 
    {
        PositionCopy = transform.position;
    }

    public void Serialize(BitWriter writer) 
    {
        writer.WriteFloat(PositionCopy.x, min, max, step);
        writer.WriteFloat(PositionCopy.y, min, max, step);
        writer.WriteFloat(PositionCopy.z, min, max, step);
        return;
    }

    public void Deserialize(BitReader reader)
    {
        Vector3 vector;
        vector.x = reader.ReadFloat(min, max, step);
        vector.y = reader.ReadFloat(min, max, step);
        vector.z = reader.ReadFloat(min, max, step);
        transform.position = vector;
        return;        
    }
}