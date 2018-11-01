using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System;
using UnityEngine;

public class HostManager : MonoBehaviour {
    private float _LocalPort;
    private float _PacketLossProbability;
    private int _TickRate;
    private float _SpinLockMargin;
    private int _SpinLockSleepTime;
    private System.Random _Random;


    private List<EndPoint> _RemoteEndPoints;
    private float _TickTime;
    private Thread _ServerThread;


	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RealStart()
    {

    }

    private void OnDisable()
    {
        if (_ServerThread != null)
        {
            _ServerThread.Abort();

        }
    }

    public void ServerSendingThreadProcedure()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();

        try
        {
            stopwatch.Start();
            while(true)
            {
                if (stopwatch.Elapsed.TotalMilliseconds < _TickTime)
                {
                    if (stopwatch.Elapsed.TotalMilliseconds + _SpinLockMargin < _TickTime)
                    {
                        Thread.Sleep(_SpinLockSleepTime);
                    }
                } else
                {
                    foreach(var endpoint in _RemoteEndPoints)
                    {
                        if (_Random.NextDouble() >= _PacketLossProbability)
                        {
                            var packet = SendNextPacket(endpoint);
                        }
                    }
                    stopwatch.Reset();
                    stopwatch.Start();
                }
            }
        } catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void ServerReceivingThreadProcedure()
    {

    }

    public Packet SendNextPacket(EndPoint remoteEndPoint)
    {
        return null;
    }
}
