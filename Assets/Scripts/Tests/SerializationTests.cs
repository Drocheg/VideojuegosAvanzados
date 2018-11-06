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
        writer.WriteInt(15, 0, 15);

        writer.WriteBit(false);
	    writer.WriteFloat(15.3f, 10.0f, 20.0f, 0.5f);
		writer.WriteBit(true);
        writer.WriteInt(500, 0, 500);
	    var testString = "Hola que tal este es un string de testing \n JAJA";
	    writer.WriteString(testString);
        writer.WriteBit(true);

		for (uint i = 0; i < 500; i++) {
			if (i % 3 == 0) {
				writer.WriteBit(i % 2 == 0);
			} else {
				writer.WriteInt(i, 0, 500);
			}
		}
		writer.Flush();
		writer.Reset();
        var reader = new BitReader(writer.GetBuffer());
        for (int i = 0; i < 13; i++) {
            Assert.True(reader.ReadBit());
        }
        Assert.AreEqual(15, reader.ReadInt(0, 15));
        Assert.False(reader.ReadBit());
	    var result = reader.ReadFloat(10.0f, 20.0f, 0.5f);
	    Assert.True(result < 15.3 + 0.5 + 0.00001 && result > 15.3 - 0.5 - 0.00001);
	    Assert.True(reader.ReadBit());
        Assert.AreEqual(500, reader.ReadInt(0, 500));
	    Assert.AreEqual(reader.ReadString(testString.Length), testString);
		Assert.True(reader.ReadBit());

		for (int i = 0; i < 500; i++) {
			if (i % 3 == 0) {
				Assert.AreEqual(i % 2 == 0, reader.ReadBit());
			} else {
				Assert.AreEqual(i, reader.ReadInt(0, 500));
			}
		}

    }
	
	[Test]
	public void SerializationTestSingleString() {
		// Use the Assert class to test conditions.
		var writer = new BitWriter(1000);
		writer.WriteString("Hola");
		writer.Flush();
		writer.Reset();
		var reader = new BitReader(writer.GetBuffer());
		var read = reader.ReadString(4);
		read.Equals("Hola");
		Assert.AreEqual(read, "Hola");
	}

    [Test]
    public void SerializationTestSingleBitTrue() {
        // Use the Assert class to test conditions.
        var writer = new BitWriter(1000);
        writer.WriteBit(true);
        writer.Flush();
		writer.Reset();
        var reader = new BitReader(writer.GetBuffer());
        Assert.True(reader.ReadBit());
    }
	
	[Test]
	public void SerializationTestSingleFloat() {
		// Use the Assert class to test conditions.
		var writer = new BitWriter(1000);
		writer.WriteFloat(15.5f, 10.0f, 20.0f, 0.5f);
		writer.Flush();
		writer.Reset();
		var reader = new BitReader(writer.GetBuffer());
		var result = reader.ReadFloat(10.0f, 20.0f, 0.5f);
		Assert.True(result < 15.50005 && result > 15.49995);
	}
	
		
	[Test]
	public void SerializationTestSingleFloatWhenStepDoNoDivideMax() {
		// Use the Assert class to test conditions.
		var writer = new BitWriter(1000);
		writer.WriteFloat(20.0f, 10.0f, 20.3f, 0.5f);
		writer.Flush();
		writer.Reset();
		var reader = new BitReader(writer.GetBuffer());
		var result = reader.ReadFloat(10.0f, 20.3f, 0.5f);
		Assert.True(result < 20.00005 && result > 19.99995);
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
