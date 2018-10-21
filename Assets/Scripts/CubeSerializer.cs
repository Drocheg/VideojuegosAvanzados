using System.Collections.Generic;
using UnityEngine;

public class CubeSerializer : MonoBehaviour, ISerial
{
    public float Min, Max, Step;
    private float _min, _max, _step;
    private bool PositionChanged;
    private Vector3 Position;
    private Quaternion Rotation;
    private Quaternion CameraRotation;
    public Animator _animator;
    public Transform Camera;
    private float timestamp;
    public float MaxTime;
    private float _maxTime;
    private bool UpdatePending;
    private float _speed, _strafe;
    public void Start()
    {
        SnapshotSerializer.GetInstance().AddReference(0, this);
        _min = Min;
        _max = Max;
        _step = Step;
        _maxTime = MaxTime;
    }

    public void Update()
    {
        timestamp += Time.deltaTime;
        Position = transform.position;
        Rotation = transform.rotation;
        CameraRotation = Camera.rotation;
        _strafe = _animator.GetFloat("Strafe");
        _speed = _animator.GetFloat("Speed");
    }

    public void Serialize(BitWriter writer)
    {
        writer.WriteFloat(Position.x, _min, _max, _step);
        writer.WriteFloat(Position.y, _min, _max, _step);
        writer.WriteFloat(Position.z, _min, _max, _step);
        writer.WriteFloat(_strafe, -1, 1, 0.1f);
        writer.WriteFloat(_speed, -1, 1, 0.1f);
        writer.WriteFloat(Rotation.w, -1, 1, 0.01f);
        writer.WriteFloat(CameraRotation.x, -1, 1, 0.01f);
        writer.WriteFloat(Rotation.y, -1, 1, 0.01f);
        writer.WriteFloat(CameraRotation.z, -1, 1, 0.01f);
        writer.WriteFloat(timestamp, 0, _maxTime, 0.001f);
        return;
    }

    public void Deserialize(BitReader reader)
    {
        Vector3 vector;
        Debug.Log(_min);
        Debug.Log(_max);
        Debug.Log(_step);
        vector.x = reader.ReadFloat(_min, _max, _step);
        vector.y = reader.ReadFloat(_min, _max, _step);
        vector.z = reader.ReadFloat(_min, _max, _step);
        PositionChanged = true;
        Debug.Log(vector);
        return;
    }
}