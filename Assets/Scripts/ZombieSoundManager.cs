using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSoundManager : MonoBehaviour {
	public List<AudioClip> ZombieSounds;
	public AudioSource ZombieSource;
	public float PauseTimeMin, PauseTimeMax;
	private float LoopTime;
	private float _time;
	private int _previousSound = -1;
	private float _pauseTime;
	// Use this for initialization
	void Start () {
		PlayZombieSound();
		_pauseTime = PauseTime();
	}
	private float PauseTime()
	{
		return Random.value * (PauseTimeMax - PauseTimeMin) + PauseTimeMin;
	}
	
	// Update is called once per frame
	void Update () {
		_time += Time.deltaTime;
		if (_time > LoopTime + _pauseTime) {
			_time = 0;
			_pauseTime = PauseTime();
			PlayZombieSound();
		}
	}

	void PlayZombieSound() {
		int nextSound;
		do {
			nextSound = (int) (Random.value * ZombieSounds.Count);
		} while(nextSound == _previousSound);
		_previousSound = nextSound;
		ZombieSource.clip = ZombieSounds[nextSound];
		LoopTime = ZombieSounds[nextSound].length;
		ZombieSource.Play();
	}
}
