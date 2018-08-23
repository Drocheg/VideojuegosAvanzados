﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPServer : MonoBehaviour
{

	public int Port_client;
	private Queue<String> messages;
	private UdpClient udpClient;
	private IPEndPoint RemoteIpEndPoint;
	private EndPoint RemoteEndPoint;
	private Thread theUDPServer;

	private byte[] byteArray;
	// Use this for initialization
	void Start()
	{
		messages = new Queue<string>();
		udpClient = new UdpClient(Port_client);
		RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port_client);
		RemoteEndPoint = (EndPoint)RemoteIpEndPoint;
		byteArray = new byte[1000];
		theUDPServer = new Thread(new ThreadStart(serverThread));
		theUDPServer.Start();
	}

	private void OnDisable()
	{
		theUDPServer.Abort();
		udpClient.Close();
	}

//	private void OnApplicationQuit()
//	{
//		theUDPServer.Abort();	
//		udpClient.Close();
//	}


	public void serverThread()
	{
		try
		{
			while (true)
			{
		//		Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
		//		int receiveBytes = udpClient.Client.ReceiveFrom(byteArray, ref RemoteIpEndPoint);
		//		string returnData = Encoding.ASCII.GetString(receiveBytes);
				
				int receiveBytes = udpClient.Client.ReceiveFrom(byteArray, ref RemoteEndPoint);
				string returnData = Encoding.ASCII.GetString(byteArray);
				
				Debug.Log("Before enqueueing msg");
				
				lock (messages)
				{
					messages.Enqueue(returnData);
				}
			}
		}
		catch (Exception e)
		{
			print(e);
		}
		
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			lock (messages)
			{
				Debug.Log(messages.Dequeue());
			}
		}
	}
}
