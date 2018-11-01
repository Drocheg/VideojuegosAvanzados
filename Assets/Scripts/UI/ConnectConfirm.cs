using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectConfirm : MonoBehaviour {
	public TMP_InputField PlayerName;
	public TMP_InputField IpAddress;
	public TMP_InputField Port;
	private Button _button;
	// Use this for initialization
	void Start () {
		_button = GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
		if (PlayerName.text != "" && IpAddress.text != "" && Port.text != "") {
			_button.interactable = true;
		} else {
			_button.interactable = false;
		}
	}
}
