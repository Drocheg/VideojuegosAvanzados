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
	public double PacketLossProbability;
	private double TickTime;
	private UdpClient udpClient;
	private IPEndPoint RemoteIpEndPoint;
	private EndPoint RemoteEndPoint;
	private Thread theUDPServer;
	private SnapshotSerializer serializer;
	private byte[] byteArray;
	private System.Random random;
	// Use this for initialization
	void Start()
	{
		random = new System.Random();
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

		// Random wait because first sent packets are repeated otherwise for some reason. Investigate further.
		// System.Threading.Thread.Sleep(1000);
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
					if(random.NextDouble() >= PacketLossProbability) {
                        BitWriter bitWriter = new BitWriter(1000);
						serializer.Serialize(bitWriter);
						int bytes = udpClient.Client.SendTo(bitWriter.GetBuffer().GetBuffer(), (int) bitWriter.GetBuffer().Length, SocketFlags.None, RemoteEndPoint);
					}
					stopwatch.Reset();
					stopwatch.Start();
				}
			}
		}
		catch (System.Exception e)
		{
			print(e);
		}

	}

	private void Update()
	{
	}
}
