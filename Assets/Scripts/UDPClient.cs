using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPClient : MonoBehaviour
{
	public string Ip_raies;
	public int Port_raies;
    private UdpClient udpClient;

	void Start () {
	    udpClient = new UdpClient();
        udpClient.Connect(Ip_raies, Port_raies);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.S))
		{
			Byte[] sendBytes = Encoding.ASCII.GetBytes("Is anybody there?");
	        udpClient.Send(sendBytes, sendBytes.Length);
			Debug.Log("Sended!");
		}

	}
	private void OnDisable()
	{
		udpClient.Close();
	}
	
	private void OnApplicationQuit()
	{
//		udpClient.Close();
	}
}
