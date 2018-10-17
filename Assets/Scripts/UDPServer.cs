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

	public int LocalPort;
	public int RemotePort;
	public string RemoteIp;
	public int TickRate;
	public double SpinLockMargin; 
	public int SpinLockSleepTime;
	private double TickTime;
	private UdpClient udpClient;
	private IPEndPoint RemoteIpEndPoint;
	private EndPoint RemoteEndPoint;
	private Thread theUDPServer;
	private SnapshotSerializer serializer;

	private byte[] byteArray;
	// Use this for initialization
	void Start()
	{
		TickTime = 1000.0 / TickRate;
		udpClient = new UdpClient(LocalPort);
		RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(RemoteIp), RemotePort);
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
		var stopwatch = new System.Diagnostics.Stopwatch();
		try
		{
			stopwatch.Start();
			while (true)
			{
				if (stopwatch.Elapsed.TotalMilliseconds < TickTime) {
					if (stopwatch.Elapsed.TotalMilliseconds + SpinLockMargin < TickTime) {
						System.Threading.Thread.Sleep(SpinLockSleepTime);
					}
				} else {
					var packet = serializer.Serialize();
					int bytes = udpClient.Client.SendTo(packet.buffer.GetBuffer(), (int) packet.buffer.Length, SocketFlags.None, RemoteEndPoint);
					Debug.Log("Time elapsed: " + stopwatch.ElapsedMilliseconds);
					stopwatch.Reset();
					stopwatch.Start();
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
	}
}
