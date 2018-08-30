using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.IO;

public class UDPClient : MonoBehaviour
{
	public string Ip_server;
	public int Port_server;
	private UdpClient udpClient;
	private MemoryStream ms;

	private BitWriter bitWriter;
	private BitReader bitReader;
	
	//private BitWriter bitReader;

	void Start()
	{
		udpClient = new UdpClient();
		udpClient.Connect(Ip_server, Port_server);
		ms = new MemoryStream();
		bitWriter = new BitWriter(ms);		
		bitReader = new BitReader(ms);
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			for (var i = 0; i < 32; i++)
			{
				bitWriter.WriteBit(i % 2 == 0);
			}
			//bitWriter.Flush();
			Debug.Log(ms.Length);
			// udpClient.Send(ms.GetBuffer(), (int) ms.Length);
			Debug.Log("Sent!");
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			Debug.Log(bitReader.ReadBit());
		}
	}
	
	private void OnDisable()
	{
		udpClient.Close();
	}
}
