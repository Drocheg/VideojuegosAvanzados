using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartHosting : MonoBehaviour {
	public TMP_InputField PlayerName;
	public TMP_InputField Port;

	public enum ConnectionState {
		NOT_READY,
		WAITING_CONFIRMATION,
		WAITING_CONNECTION,
		READY,
	}
	private ConnectionState _state;
	private Button _button;
	private bool _buttonPressed, _gameIsReadyToStart, _gameStarted;
	// Use this for initialization
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
				if (PlayerName.text != "" && Port.text != "") {
					_state = ConnectionState.WAITING_CONFIRMATION;
					_button.interactable = true;
				}
				break;
			}
			case ConnectionState.WAITING_CONFIRMATION: {
				if (_buttonPressed) {
					_state = ConnectionState.WAITING_CONNECTION;
					_button.interactable = false;
					_button.GetComponentInChildren<Text>().text = "Started";
					MenuVariables.MenuName = PlayerName.text;
					MenuVariables.MenuPort = Int32.Parse(Port.text);
					SceneManager.LoadScene("scenes/NiceSceneServer");
					//_button.GetComponentInChildren<Text>().text = "Listening";
					//StartCoroutine(WaitTillConnect()); // TODO could be changed to wait for players.
				}
				break;
			}
/*			case ConnectionState.WAITING_CONNECTION: {
				if (_gameIsReadyToStart) {
					_button.interactable = true;
					_button.GetComponentInChildren<Text>().text = "Start";
					_button.onClick.AddListener(() => _gameStarted = true);
					_state = ConnectionState.READY;
				}
				break;
			}
			case ConnectionState.READY: {
				if (_gameStarted) {
					_button.interactable = false;
					_button.GetComponentInChildren<Text>().text = "Started";
					SceneManager.LoadScene("ServerCube");
				}
				break;
			}
*/
		}	
	}

	IEnumerator WaitTillConnect() {
			yield return new WaitForSeconds(5);
		_gameIsReadyToStart = true;
	}
}

