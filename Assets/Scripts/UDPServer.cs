using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine;

public class UDPServer : MonoBehaviour
{

	public int Port_client;
	public string IpClient;
	private UdpClient udpClient;
	private IPEndPoint RemoteIpEndPoint;
	private EndPoint RemoteEndPoint;
	private Thread theUDPServer;
	private SnapshotSerializer serializer;

	private byte[] byteArray;
	// Use this for initialization
	void Start()
	{
		udpClient = new UdpClient(Port_client);
		RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IpClient), Port_client);
		RemoteEndPoint = (EndPoint)RemoteIpEndPoint;
		serializer = SnapshotSerializer.GetInstance();
		theUDPServer = new Thread(new ThreadStart(serverThread));
		theUDPServer.Start();
	}

	private void OnDisable()
	{
		theUDPServer.Abort();
		udpClient.Close();
	}

	public void serverThread()
	{
		try
		{
			while (true)
			{
				var packet = serializer.Serialize();
				int bytes = udpClient.Client.SendTo(packet.buffer.GetBuffer(), (int) packet.buffer.Length, SocketFlags.None, RemoteEndPoint);
				Debug.Log(packet.buffer.GetBuffer());
				Debug.Log("Bytes sent: " + bytes);
				System.Threading.Thread.Sleep(15);
			}
		}
		catch (Exception e)
		{
			print(e);
		}

	}

	private void Update()
	{
	}
}
