﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.IO;

public class UDPClient : MonoBehaviour
{
	public string Ip_raies;
	public int Port_raies;
    private UdpClient udpClient;
    private MemoryStream ms;
    private BitWriter bitWriter;

	void Start () {
	    udpClient = new UdpClient();
        udpClient.Connect(Ip_raies, Port_raies);
        ms = new MemoryStream();
        bitWriter = new BitWriter(ms);
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.S))
		{
            bitWriter.WriteBit(true);
            bitWriter.Flush();
            Debug.Log(ms.Length);
	        udpClient.Send(ms.GetBuffer(), (int) ms.Length);
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
