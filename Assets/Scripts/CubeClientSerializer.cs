using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class CubeClientSerializer: MonoBehaviour, ISerial
{

    // All Serialization parameters should be given by the server on first negotiation.
    public float PositionSerialMin, PositionSerialMax, PositionSerialStep;
    public float RotationSerialStep;
    public float AnimationSerialStep;
    public float TimeSerialStep;
    public NetworkState CurrentState;
    public int MaxQueuedPositions;
    public int MinQueuedPositions;
    private bool PositionChanged;
    private Vector3 PositionCopy;
    public Vector3? PreviousPosition;
    public float PreviousTime;
    public Vector3? NextPosition;
    public float NextTime;
    private Queue<Vector3DeltaTime> QueuedPositions;
    private Queue<Vector2> QueuedAnimations; 
    public float CurrentTime;

    public float MaxTime;
    private float _maxTime;
    public float DELTA_TIME;

    private Vector2? PreviousAnimation, NextAnimation;
    private Quaternion? PreviousRotation, NextRotation, CurrentRotation;

	private Animator _animator;
    private Transform _chest;
    class Vector3DeltaTime
    {
        public Vector3 pos;
        public float time;
        public Vector2 animation;
        public Quaternion rot;
    }

    public void Start()
    {
        SnapshotSerializer.GetInstance().AddReference(0, this);
        QueuedPositions = new Queue<Vector3DeltaTime>(3);
        _maxTime = MaxTime;
		_animator = GetComponent<Animator>();
    }

    public enum NetworkState {
        INITIAL,
        NORMAL,
        NETWORK_PROBLEMS,
    }
    private bool DequeNextPosition(out Vector3? deqPosition, out float deqTime, out Vector2? animation, out Quaternion? rot)
    {
        if (QueuedPositions.Count > 0){
            var wrapper = QueuedPositions.Dequeue();
            deqPosition = wrapper.pos;
            deqTime = wrapper.time;
            animation = wrapper.animation;
            rot = wrapper.rot;
            return true;
        }
        deqPosition = null;
        deqTime = 0;
        animation = null;
        rot = null;
        return false;
    }

    private void LerpAnimation(Vector2 prevAnim, Vector2 nextAnim, float d)
    {
        var anim = Vector2.Lerp(prevAnim, nextAnim, d);
        _animator.SetFloat("Strafe", anim.x);
        _animator.SetFloat("Speed", anim.y);
        _chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
    }

    public void Update()
    {
        Debug.Log("Prev: " + PreviousTime + ", Next: " + NextTime);
        switch(CurrentState) {
            case NetworkState.INITIAL: {
                // Initial position arrived but not enough info to interpolate.
                Debug.Assert(PreviousPosition == null);
                Debug.Assert(NextPosition == null);
                Debug.Assert(CurrentTime == 0);
                if (QueuedPositions.Count >= MinQueuedPositions) {
                    Debug.Assert(QueuedPositions.Count >= 2);
                    DequeNextPosition(out PreviousPosition, out PreviousTime, out PreviousAnimation, out PreviousRotation);
                    DequeNextPosition(out NextPosition, out NextTime, out NextAnimation, out NextRotation);
                    CurrentTime = PreviousTime;
                    // Enough info to interpolate now.
                    transform.position = Vector3.Lerp(PreviousPosition.Value, NextPosition.Value, 0);
                    LerpAnimation(PreviousAnimation.Value, NextAnimation.Value, 0);
                    CurrentRotation = Quaternion.Lerp(PreviousRotation.Value, NextRotation.Value, 0);
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
                    Vector2? auxAnimation;
                    Quaternion? auxRotation;
                    if (DequeNextPosition(out auxPos, out auxTime, out auxAnimation, out auxRotation)) {
                        PreviousPosition = NextPosition;
                        PreviousTime = NextTime;
                        PreviousAnimation = NextAnimation;
                        PreviousRotation = NextRotation;
                        NextPosition = auxPos;
                        NextTime = auxTime;
                        NextAnimation = auxAnimation;
                        NextRotation = auxRotation;
                    } else {
                        // There is no more info to interpolate. Network problems state.
                        transform.position = Vector3.Lerp(PreviousPosition.Value, NextPosition.Value, 1);
                        LerpAnimation(PreviousAnimation.Value, NextAnimation.Value, 1);
                        CurrentRotation = Quaternion.Lerp(PreviousRotation.Value, NextRotation.Value, 1);
                        break;
                    }
                }
				var d = (CurrentTime - PreviousTime) / (NextTime - PreviousTime);
                transform.position = Vector3.Lerp(PreviousPosition.Value, NextPosition.Value, d);
                LerpAnimation(PreviousAnimation.Value, NextAnimation.Value, d);
                CurrentRotation = Quaternion.Lerp(PreviousRotation.Value, NextRotation.Value, d);
                break;
            }
			case NetworkState.NETWORK_PROBLEMS: {
				break;
			}
        }
    }

    void LateUpdate() 
    {
        if (CurrentRotation.HasValue) {
            var chestRot = _chest.transform.rotation;
            var rot = transform.rotation;
            rot.w = CurrentRotation.Value.w;
            chestRot.x = CurrentRotation.Value.x;
            rot.y = CurrentRotation.Value.y;
            chestRot.z = CurrentRotation.Value.z;
            _chest.transform.rotation = chestRot;
            transform.rotation = rot;
        }
    }

    public void QueueNextPosition(Vector3 nextPos, float nextTime, Vector2 anim, Quaternion rot)
    {
        if (QueuedPositions.Count >= MaxQueuedPositions) {
            Debug.Log("Dropping queued positions");
            QueuedPositions.Dequeue();
        }
        QueuedPositions.Enqueue(new Vector3DeltaTime() { pos = nextPos, time = nextTime, animation = anim, rot = rot });
    }

    public void Serialize(BitWriter writer) {
        writer.WriteFloat(PositionCopy.x, PositionSerialMin, PositionSerialMax, PositionSerialStep);
        writer.WriteFloat(PositionCopy.y, PositionSerialMin, PositionSerialMax, PositionSerialStep);
        writer.WriteFloat(PositionCopy.z, PositionSerialMin, PositionSerialMax, PositionSerialStep);
        return;
    }

    public void Deserialize(BitReader reader)
    {
        Vector3 vector;
        vector.x = reader.ReadFloat(PositionSerialMin, PositionSerialMax, PositionSerialStep);
        vector.y = reader.ReadFloat(PositionSerialMin, PositionSerialMax, PositionSerialStep);
        vector.z = reader.ReadFloat(PositionSerialMin, PositionSerialMax, PositionSerialStep);
        Vector2 anim;
        anim.x = reader.ReadFloat(-1, 1, AnimationSerialStep);
        anim.y = reader.ReadFloat(-1, 1, AnimationSerialStep);
        Quaternion rot;
        rot.w = reader.ReadFloat(-1, 1, RotationSerialStep);
        rot.x = reader.ReadFloat(-1, 1, RotationSerialStep);
        rot.y = reader.ReadFloat(-1, 1, RotationSerialStep);
        rot.z = reader.ReadFloat(-1, 1, RotationSerialStep);
        float time = reader.ReadFloat(0, _maxTime, TimeSerialStep);
        QueueNextPosition(vector, time, anim, rot);
    }
}
