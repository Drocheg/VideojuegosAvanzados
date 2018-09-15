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
	PacketReceiver packetReceiver;
	
	private UdpClient udpClient;
	private MemoryStream ms;

	private BitWriter bitWriter;
	private BitReader bitReader;
    private bool reading;

	
	
	//private BitWriter bitReader;

	void Start()
	{
		//udpClient = new UdpClient();
		//udpClient.Connect(Ip_server, Port_server);
		bitWriter = new BitWriter(1000);
		packetReceiver = GetComponent<PacketReceiver>();
	}

	// Update is called once per frame
	void Update()
	{
		
	
		
		if (Input.GetKeyDown(KeyCode.S))
		{
            if (reading)
            {
                reading = false;
                ms.Flush();
                ms.Position = 0;
            }
			for (var i = 0; i < 8; i++)
			{
				bitWriter.WriteBit(i % 2 == 0);
			}
            bitWriter.GetBuffer();
			Debug.Log("Sent!");
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
            bitWriter.Flush();
            var ms = bitWriter.GetBuffer();
            int position = (int) ms.Position;
            var buffer = new byte[position];
            ms.Position = 0;
            ms.Read(buffer, 0, position);
            bitReader = new BitReader(new MemoryStream(buffer));
			for (var i = 0; i < 8; i++)
            {
                Debug.Log(bitReader.ReadBit());
            }
            bitWriter.ResetBuffer();
		}


		if (Input.GetKeyDown(KeyCode.N))
		{
			// send snapshot
			
		}
		
		if (Input.GetKeyDown(KeyCode.E))
		{
			
			
		}
	}
	
	private void OnDisable()
	{
		//udpClient.Close();
	}
}
