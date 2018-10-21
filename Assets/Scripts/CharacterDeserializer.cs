using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class CharacterDeserializer: MonoBehaviour, ISerial
{

    // All Serialization parameters should be given by the server on first negotiation.
    public float PositionSerialMin, PositionSerialMax, PositionSerialStep;
    public float RotationSerialStep;
    public float AnimationSerialStep;
    public float TimeSerialStep, TimeSerialMax;
    public int MinQueuedPositions, MaxQueuedPositions;
    private NetworkState _currentState;
    private Vector3? _previousPosition, _nextPosition;
    private float _previousTime, _nextTime, _currentTime;
    private Queue<Vector3DeltaTime> _queuedPositions;
    private Vector2? _previousAnimation, _nextAnimation;
    private Quaternion? _previousRotation, _nextRotation, _currentRotation;
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
        _queuedPositions = new Queue<Vector3DeltaTime>(MaxQueuedPositions);
		_animator = GetComponent<Animator>();
    }

    public enum NetworkState {
        INITIAL,
        NORMAL,
        NETWORK_PROBLEMS,
    }
    private bool DequeNextPosition(out Vector3? deqPosition, out float deqTime, out Vector2? animation, out Quaternion? rot)
    {
        if (_queuedPositions.Count > 0){
            var wrapper = _queuedPositions.Dequeue();
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
        switch(_currentState) {
            case NetworkState.INITIAL: {
                // Initial position arrived but not enough info to interpolate.
                if (_queuedPositions.Count >= MinQueuedPositions) {
                    Debug.Assert(_queuedPositions.Count >= 2);
                    DequeNextPosition(out _previousPosition, out _previousTime, out _previousAnimation, out _previousRotation);
                    DequeNextPosition(out _nextPosition, out _nextTime, out _nextAnimation, out _nextRotation);
                    _currentTime = _previousTime;
                    // Enough info to interpolate now.
                    transform.position = Vector3.Lerp(_previousPosition.Value, _nextPosition.Value, 0);
                    LerpAnimation(_previousAnimation.Value, _nextAnimation.Value, 0);
                    _currentRotation = Quaternion.Lerp(_previousRotation.Value, _nextRotation.Value, 0);
                    _currentState = NetworkState.NORMAL;
                    Debug.Log("Reset. CurrentTime: " + _currentTime + " , NextTime: " + _nextTime);
                }
                break;
            }
            case NetworkState.NORMAL: {
                // var timeMultiplier = _queuedPositions.Count < MinQueuedPositions ? 0.9f: _queuedPositions.Count > MaxQueuedPositions ? 1.1f : 1;
                // Debug.Log("TimeM: " + timeMultiplier);
                Debug.Log("Q: " + _queuedPositions.Count);
                _currentTime += Time.deltaTime ;
                if (_currentTime > _nextTime) {
                    // Deque next position
                    Vector3? auxPos;
                    float auxTime;
                    Vector2? auxAnimation;
                    Quaternion? auxRotation;
                    if (DequeNextPosition(out auxPos, out auxTime, out auxAnimation, out auxRotation)) {
                        _previousPosition = _nextPosition;
                        _previousTime = _nextTime;
                        _previousAnimation = _nextAnimation;
                        _previousRotation = _nextRotation;
                        _nextPosition = auxPos;
                        _nextTime = auxTime;
                        _nextAnimation = auxAnimation;
                        _nextRotation = auxRotation;
                    } else {
                        // There is no more info to interpolate. Network problems state.
                        transform.position = Vector3.Lerp(_previousPosition.Value, _nextPosition.Value, 1);
                        LerpAnimation(_previousAnimation.Value, _nextAnimation.Value, 1);
                        _currentRotation = Quaternion.Lerp(_previousRotation.Value, _nextRotation.Value, 1);
                        _currentState = NetworkState.INITIAL;
                        Debug.Log("Network Problems");
                        break;
                    }
                }
				var d = (_currentTime - _previousTime) / (_nextTime - _previousTime);
                transform.position = Vector3.Lerp(_previousPosition.Value, _nextPosition.Value, d);
                LerpAnimation(_previousAnimation.Value, _nextAnimation.Value, d);
                _currentRotation = Quaternion.Lerp(_previousRotation.Value, _nextRotation.Value, d);
                break;
            }
			case NetworkState.NETWORK_PROBLEMS: {
				break;
			}
        }
    }

    void LateUpdate() 
    {
        if (_currentRotation.HasValue) {
            var chestRot = _chest.transform.rotation;
            var rot = transform.rotation;
            rot.w = _currentRotation.Value.w;
            chestRot.x = _currentRotation.Value.x;
            rot.y = _currentRotation.Value.y;
            chestRot.z = _currentRotation.Value.z;
            _chest.transform.rotation = chestRot;
            transform.rotation = rot;
        }
    }

    public void QueueNextPosition(Vector3 nextPos, float nextTime, Vector2 anim, Quaternion rot)
    {
        if (_queuedPositions.Count >= MaxQueuedPositions) {
            Debug.Log("Dropping queued positions");
            _queuedPositions.Dequeue();
        }
        _queuedPositions.Enqueue(new Vector3DeltaTime() { pos = nextPos, time = nextTime, animation = anim, rot = rot });
    }

    public void Serialize(BitWriter writer) {
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
        float time = reader.ReadFloat(0, TimeSerialMax, TimeSerialStep);
        QueueNextPosition(vector, time, anim, rot);
    }
}
