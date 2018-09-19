using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	public float speed = 10;
	private Transform t;
	
	// Use this for initialization
	void Start ()
	{
		t = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKey(KeyCode.UpArrow))
		{
			transform.Translate(Vector3.up * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			transform.Translate(Vector3.right * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			transform.Translate(Vector3.left * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			transform.Translate(Vector3.down * Time.deltaTime * speed);
		}
	}
}
