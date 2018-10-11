using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

class CubeClientSerializer: MonoBehaviour
{
    public float Min, Max, Step;
    private float _min, _max, _step;
    private bool PositionChanged;
    private Vector3 PositionCopy;
    public Vector3? PreviousPosition;
    public float PreviousTime;
    public Vector3? NextPosition;
    public float NextTime;
    private Queue<Vector3DeltaTime> QueuedPositions;
    public float CurrentTime;


    class Vector3DeltaTime
    {
        public Vector3 pos;
        public float time;
    }


    public void Start()
    {
        SnapshotSerializer.GetInstance().AddReference(0, this);
        _min = Min;
        _max = Max;
        _step = Step;
    }

    private void DequeNextPosition()
    {
        if (QueuedPositions.Count > 0)
        {
            var wrapper = QueuedPositions.Dequeue();
            NextPosition = wrapper.pos;
            NextTime = wrapper.time;
        }
        else
        {
            NextPosition = null;
            NextTime = 0;
        }
    }

    public void Update()
    {
        if (PreviousPosition != null)
        {
            CurrentTime += Time.deltaTime;

            if (NextPosition == null)
            {
                DequeNextPosition();
            }
            if (NextPosition != null)
            {
                PositionChanged = true;
                if (CurrentTime < NextTime)
                {
                    transform.position = Vector3.Lerp(PreviousPosition.Value, NextPosition.Value, (CurrentTime - PreviousTime) / (NextTime - PreviousTime));
                }
                else
                {
                    transform.position = Vector3.Lerp(PreviousPosition.Value, NextPosition.Value, 1);
                    PreviousPosition = NextPosition;
                    DequeNextPosition();
                }
            }
        }
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

    public void QueueNextPosition(Vector3 nextPos, float nextTime)
    {
        QueuedPositions.Enqueue(new Vector3DeltaTime() { pos = nextPos, time = nextTime });
    }

    public void Serialize(BitWriter writer)
    {
        writer.WriteFloat(PositionCopy.x, _min, _max, _step);
        writer.WriteFloat(PositionCopy.y, _min, _max, _step);
        writer.WriteFloat(PositionCopy.z, _min, _max, _step);
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
