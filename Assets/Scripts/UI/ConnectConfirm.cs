using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ConnectConfirm : MonoBehaviour {
	public TMP_InputField PlayerName;
	public TMP_InputField IpAddress;
	public TMP_InputField Port;
	private Button _button;
	private ConnectionState _state;
	private bool _buttonPressed, _gameIsReadyToStart, _gameStarted;
	
	public enum ConnectionState {
		NOT_READY,
		WAITING_CONFIRMATION,
		WAITING_CONNECTION,
	}
	
	void Start () {
		_state = ConnectionState.NOT_READY;
		_button = GetComponent<Button>();
		_button.interactable = false;
		_button.onClick.AddListener(() => _buttonPressed = true);
	}
	
	// Update is called once per frame
	void Update () {
		switch(_state) {
			case ConnectionState.NOT_READY: {
				if (PlayerName.text != "" && IpAddress.text != "" && Port.text != "") {
					_state = ConnectionState.WAITING_CONFIRMATION;
					_button.interactable = true;
				}
				break;
			}
			case ConnectionState.WAITING_CONFIRMATION: {
				if (_buttonPressed) {
					_state = ConnectionState.WAITING_CONNECTION;
					_button.interactable = false;
					_button.GetComponentInChildren<Text>().text = "Connecting";
				}
				break;
			}
			case ConnectionState.WAITING_CONNECTION: {			
				_button.interactable = false;
				_button.GetComponentInChildren<Text>().text = "Started";
				MenuVariables.MenuName = PlayerName.text;
				MenuVariables.MenuPort = Int32.Parse(Port.text);
				MenuVariables.MenuIP = IpAddress.text;
				SceneManager.LoadScene("scenes/NiceLevelClient");
				break;
			}
		}	
	}
	
}
