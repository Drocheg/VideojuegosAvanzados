using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[TestFixture] 
public class TestNetworkApi
{
    private NetworkAPI networkApi;
    private EndPoint endPoint0;
    private uint channelId0;
    private int port;

    [TestFixtureSetUp]
    public void Init()
    {
        port = 55555;
        networkApi = NetworkAPI.GetInstance();
        endPoint0 = new IPEndPoint(IPAddress.Parse("10.10.10.10"), port);
        channelId0 = 0;
    }

    [TestFixtureTearDown]
    public void Cleanup()
    {
          
    }
    
    
    
    [SetUp]
    public void RunBeforeAnyTests()
    {
        networkApi.Init(port, 10, 4, 10000);
    }
    
    [TearDown]
    public void RunAfterAnyTests()
    {
        networkApi.Close();
    }

    [Test]
    public void AddOneUnreliableChannelTest()
    {
        Assert.IsTrue(networkApi.AddUnreliableChannel(channelId0, endPoint0));
    }
    
    [Test]
    public void AddOneReliableChannelTest()
    {
        Assert.IsTrue(networkApi.AddNoTimeoutReliableChannel(channelId0, endPoint0));
    }
    
    [Test]
    public void AddInvalidChannelTest()
    {
        networkApi.AddNoTimeoutReliableChannel(channelId0, endPoint0);
        Assert.IsFalse(networkApi.AddUnreliableChannel(channelId0, endPoint0));
    }

    [Test]
    public void AddMessageToChannelSend()
    {
        networkApi.AddUnreliableChannel(channelId0, endPoint0);
        SerialObject o = new SerialObject(10, "hola", true);
        Assert.IsTrue(networkApi.Send(channelId0, endPoint0, o));
    }

    [Test]
    public void GetExistingChannel()
    {
        networkApi.AddUnreliableChannel(channelId0, endPoint0);
        NetworkChannel nc;
        Assert.IsTrue(networkApi.getChannel(channelId0, endPoint0, out nc));
        
    }
    
    [Test]
    public void SendSingleMessage()
    {
        networkApi.AddUnreliableChannel(channelId0, endPoint0);
        SerialObject o = new SerialObject(10, "hola", true);
        networkApi.Send(channelId0, endPoint0, o);
        NetworkChannel nc;
        networkApi.getChannel(channelId0, endPoint0, out nc);
        List<Packet> packets = nc.GetPacketsToSend();
        Assert.AreEqual(packets.Count, 1);
        Packet p = packets[0];
        BitReader reader = new BitReader(new MemoryStream(p.buffer));
        var channel = (uint) reader.ReadInt(0, 4);
        var seq = (uint) reader.ReadInt(0, 10000);
        var packetType = (PacketType) reader.ReadInt(0, Enum.GetNames(typeof(PacketType)).Length);
        SerialObject o2 = new SerialObject(0, "", false);
        o2.Deserialize(reader);
        Assert.AreEqual(o2, o);
    }

    private class SerialObject : ISerial
    {
        private ulong value;
        private String text;
        private bool flag;
        
        public void Serialize(BitWriter writer)
        {
            writer.WriteInt(value, 0, 1000);
            writer.WriteInt((ulong)text.Length, 0, 1000);
            writer.WriteString(text);
            writer.WriteBit(flag);
        }

        public void Deserialize(BitReader reader)
        {
            value = (ulong)reader.ReadInt(0, 1000);
            var size = reader.ReadInt(0, 1000);
            text = reader.ReadString(size);
            flag = reader.ReadBit();
        }

        protected bool Equals(SerialObject other)
        {
            return value == other.value && string.Equals(text, other.text) && flag == other.flag;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SerialObject) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = value.GetHashCode();
                hashCode = (hashCode * 397) ^ (text != null ? text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ flag.GetHashCode();
                return hashCode;
            }
        }

        public SerialObject(ulong value, string text, bool flag)
        {
            this.value = value;
            this.text = text;
            this.flag = flag;
        }

        public override string ToString()
        {
            return "Value: " + value +
                   " text: " + text +
                   " flag: " + flag;
        }
    }
    
}
