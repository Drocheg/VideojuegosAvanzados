using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritePool : MonoBehaviour {
	private Queue<SpriteRenderer> _q;
	public int PoolSize;
	public SpriteRenderer SpritePrefab;
	// Use this for initialization
	void Start () {
		_q = new Queue<SpriteRenderer>();
		for(int i = 0; i < PoolSize; i++) {
			var go = Instantiate(SpritePrefab);
			go.enabled = false;
			_q.Enqueue(go);
		}
	}
	
	public SpriteRenderer GetSprite() 
	{
		var s = _q.Dequeue();
		s.enabled = true;
		_q.Enqueue(s);
		return s;
	}
}
