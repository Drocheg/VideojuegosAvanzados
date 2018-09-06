using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class SerializationTests {

    [Test]
    public void SerializationTestsSimplePasses() {
        // Use the Assert class to test conditions.
        var writer = new BitWriter(1000000);
        for (int i = 0; i < 13; i++)
        {
            writer.WriteBit(true);
        }
        writer.WriteInt(15, 0, 32);
        writer.WriteBit(false);
		writer.WriteBit(true);
        writer.WriteInt(500, 0, 700);
		
        // writer.WriteBit(true);

		for (int i = 0; i < 500; i++) {
			if (i % 3 == 0) {
				writer.WriteBit(i % 2 == 0);
			} else {
				writer.WriteInt(i, 0, 900);
			}
		}
		writer.Flush();

        var reader = new BitReader(writer.GetBuffer());
        for (int i = 0; i < 13; i++) {
            Assert.True(reader.ReadBit());
        }
        Assert.AreEqual(15, reader.ReadInt(0, 32));
        Assert.False(reader.ReadBit());
		Assert.True(reader.ReadBit());

        Assert.AreEqual(500, reader.ReadInt(0, 32));

		for (int i = 0; i < 500; i++) {
			if (i % 3 == 0) {
				Assert.AreEqual(i % 2 == 0, reader.ReadBit());
			} else {
				Assert.AreEqual(i, reader.ReadInt(0, 900));
			}
		}



    }

    [Test]
    public void SerializationTestSingleBitTrue() {
        // Use the Assert class to test conditions.
        var writer = new BitWriter(1000);
        writer.WriteBit(true);
        writer.Flush();
        var reader = new BitReader(writer.GetBuffer());
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
