using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class CubeClientSerializer: MonoBehaviour, ISerial
{
    public float Min, Max, Step;
    public NetworkState CurrentState;
    public int MaxQueuedPositions;
    public int MinQueuedPositions;
    private float _min, _max, _step;
    private bool PositionChanged;
    private Vector3 PositionCopy;
    public Vector3? PreviousPosition;
    public float PreviousTime;
    public Vector3? NextPosition;
    public float NextTime;
    private Queue<Vector3DeltaTime> QueuedPositions;
    public float CurrentTime;

    public float MaxTime;
    private float _maxTime;
    public float DELTA_TIME;


    class Vector3DeltaTime
    {
        public Vector3 pos;
        public float time;
    }


    public void Start()
    {
        SnapshotSerializer.GetInstance().AddReference(0, this);
        QueuedPositions = new Queue<Vector3DeltaTime>(3);
        _min = Min;
        _max = Max;
        _step = Step;
        _maxTime = MaxTime;
    }

    public enum NetworkState {
        INITIAL,
        NORMAL,
        NETWORK_PROBLEMS,
    }
    private bool DequeNextPosition(out Vector3? deqPosition, out float deqTime)
    {
        if (QueuedPositions.Count > 0){
            var wrapper = QueuedPositions.Dequeue();
            deqPosition = wrapper.pos;
            deqTime = wrapper.time;
            return true;
        }
        deqPosition = null;
        deqTime = 0;
        return false;
    }

    public void Update()
    {
        switch(CurrentState) {
            case NetworkState.INITIAL: {
                // Initial position arrived but not enough info to interpolate.
                Debug.Assert(PreviousPosition == null); Debug.Assert(NextPosition == null); Debug.Assert(CurrentTime == 0);
                if (QueuedPositions.Count >= MinQueuedPositions) {
                    Debug.Assert(QueuedPositions.Count >= 2);
                    DequeNextPosition(out PreviousPosition, out PreviousTime);
                    DequeNextPosition(out NextPosition, out NextTime);
                    CurrentTime = PreviousTime;
                    // Enough info to interpolate now.
                    transform.position = Vector3.Lerp(PreviousPosition.Value, NextPosition.Value, 0);
                    CurrentState = NetworkState.NORMAL;
                }
                break;
            }
            case NetworkState.NORMAL: {
                CurrentTime += Time.deltaTime;
                if (CurrentTime > NextTime) {
                    // Deque next position
                    Vector3? auxPos;
                    float auxTime;
                    if (DequeNextPosition(out auxPos, out auxTime)) {
                        PreviousPosition = NextPosition;
                        PreviousTime = NextTime;
                        NextPosition = auxPos;
                        NextTime = auxTime;
                    } else {
                        // There is no more info to interpolate. Network problems state
                        transform.position = Vector3.Lerp(PreviousPosition.Value, NextPosition.Value, 1);
                        break;
                    }
                }
                transform.position = Vector3.Lerp(PreviousPosition.Value, NextPosition.Value, (CurrentTime - PreviousTime) / (NextTime - PreviousTime));
                break;
            }
        } 
        Debug.Log(QueuedPositions.Count);
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
        vector.x = reader.ReadFloat(_min, _max, _step);
        vector.y = reader.ReadFloat(_min, _max, _step);
        vector.z = reader.ReadFloat(_min, _max, _step);
        float time = reader.ReadFloat(0, _maxTime, 0.001f);
        QueueNextPosition(vector, time);
    }
}
