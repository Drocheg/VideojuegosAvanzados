using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour {
	private Queue<ParticleSystem> _particleSystemArray;
	public int ParticleSystemCount;
	public ParticleSystem ParticleSystemPrefab;

	// Use this for initialization
	void Start () {
		_particleSystemArray = new Queue<ParticleSystem>(ParticleSystemCount);

		for (int i = 0; i < ParticleSystemCount; i++) {
			_particleSystemArray.Enqueue(Instantiate(ParticleSystemPrefab));
		}
	}
	
	// Update is called once per frame
	public ParticleSystem GetParticleSystem()
	{
		if (_particleSystemArray.Count <= 0) {
			return null;
		}
		return _particleSystemArray.Dequeue();
	}

	public void ReleaseParticleSystem(ParticleSystem particleSystem) 
	{
		_particleSystemArray.Enqueue(particleSystem);
	}
}
