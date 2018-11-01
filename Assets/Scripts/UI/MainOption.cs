using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainOption : MonoBehaviour {

	public Canvas ConnectCanvas;
	private Canvas _mainCanvas;
	private Button _button; 
	// Use this for initialization
	void Start () {
		_button = GetComponent<Button>();
		_button.onClick.AddListener(OnClickCallback);
		_mainCanvas = GetComponentInParent<Canvas>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnClickCallback() {
		foreach(var subCanvas in _mainCanvas.GetComponentsInChildren<Canvas>()) {
			if (subCanvas == _mainCanvas) continue;
			subCanvas.enabled = false;
		}
		ConnectCanvas.enabled = true;
	}
}
