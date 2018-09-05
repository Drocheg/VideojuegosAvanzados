using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class SerializationTests {

    [Test]
    public void SerializationTestsSimplePasses() {
        // Use the Assert class to test conditions.
        var writer = new BitWriter(1000);

        for (int i = 0; i < 16; i++)
        {
            writer.WriteBit(true);
        }
        writer.WriteInt(15, 0, 32);
        writer.WriteBit(false);
        writer.WriteInt(500, 0, 700);
        writer.WriteBit(true);
        writer.Flush();

        var reader = new BitReader(writer.GetBuffer());
        for (int i = 0; i < 16; i++) {
            Assert.True(reader.ReadBit());
        }
        Assert.AreEqual(15, reader.ReadInt(0, 32));
        Assert.False(reader.ReadBit());
        Assert.AreEqual(500, reader.ReadInt(0, 32));
        Assert.True(reader.ReadBit());
    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator SerializationTestsWithEnumeratorPasses() {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }
}
