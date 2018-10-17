using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.IO;
using System.Threading;

public class UDPClient : MonoBehaviour
{
	private UdpClient udpClient;
	public int Port_server;
	
	private Thread theUDPClient;
	private SnapshotSerializer serializer;


	void Start()
	{
		udpClient = new UdpClient(Port_server);
		serializer = SnapshotSerializer.GetInstance();
		theUDPClient = new Thread(new ThreadStart(clientThread));
		theUDPClient.Start();
	}
	
	private void OnDisable()
	{
		theUDPClient.Abort();
		udpClient.Close();
	}
	
	public void clientThread()
	{
		try
		{
			while (true)
			{
				EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] buffer = new byte[1000];
				int bytes = udpClient.Client.ReceiveFrom(buffer, 1000, SocketFlags.None, ref remoteEndPoint);
				var bitReader = new BitReader(new MemoryStream(buffer));
				var packet = Packet.ReadPacket(bitReader);
				serializer.Deserialize(packet, bitReader);
			}
		}
		catch (Exception e)
		{
			print(e);
		}
		
	}
	
}
