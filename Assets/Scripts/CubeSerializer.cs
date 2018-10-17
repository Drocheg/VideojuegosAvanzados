﻿using System.Collections.Generic;
using UnityEngine;

public class CubeSerializer : MonoBehaviour, ISerial
{
    public float Min, Max, Step;
    private float _min, _max, _step;
    private bool PositionChanged;
    private Vector3 PositionCopy;
    private float timestamp;
    public float MaxTime;
    private float _maxTime;
    private bool UpdatePending;
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
        UpdatePending = true;
        if (PositionChanged)
        {
            PositionChanged = false;
            transform.position = PositionCopy;
        }
        else
        {
            PositionCopy = transform.position;
        }
    }

    public void Serialize(BitWriter writer) 
    {
        writer.WriteFloat(PositionCopy.x, _min, _max, _step);
        writer.WriteFloat(PositionCopy.y, _min, _max, _step);
        writer.WriteFloat(PositionCopy.z, _min, _max, _step);
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
        PositionCopy = vector;
        PositionChanged = true;
        Debug.Log(vector);
        return;        
    }
}