using System.IO;
using System;

public class BitWriter
{
    MemoryStream memoryStream;
    ulong bits;
    int bitCount;

    public BitWriter(int capacity)
    {
        this.memoryStream = new MemoryStream(capacity);
    }

    public BitWriter(MemoryStream memoryStream)
    {
        this.memoryStream = memoryStream;
    }

    public void WriteBit(bool value)
    {
        bits |= (value ? 1UL : 0UL) << bitCount++;
        WriteIfNecessary();
    }

    void WriteBits(ulong value, int count)
    {
        if (count > 32)
        {
            throw new Exception("Max count value supported is 32");
        }
        value = value & (ulong.MaxValue >> (64 - count));
        bits |= value << bitCount;
        bitCount += count;
        WriteIfNecessary();
    }

    public void WriteInt(ulong value, uint min, uint max)
    {
        if (value < min || value > max)
        {
            throw new Exception("Value not in a valid range.");
        }
        WriteBits(value, (int) Math.Log((double) (max - min), 2.0) + 1);
		// WriteBits(value, 32);
    }

    public void WriteFloat(float value, float min, float max, float step)
    {
        if (value < min || value > max)
        {
            throw new Exception("Value not in a valid range.");
        }
        int floatBits = (int)((max - min) / step);
        ulong longVal = (ulong)((value - min) / step);
        WriteBits(longVal, (int) Math.Log((double) floatBits, 2.0) + 1);
    }

    public void WriteString(string value)
    {
        foreach (char c in value)
        {
            WriteBits(c, 8);
        }
    }

    private void WriteIfNecessary()
    {
        if (bitCount < 32)
        {
            return;
        }

        Write32bits();
        bits >>= 32;
        bitCount -= 32;
    }

    private void Write32bits()
    {
        int val = (int) bits;
        byte a = (byte) val;
        byte b = (byte)(val >> 8);
        byte c = (byte)(val >> 16);
        byte d = (byte)(val >> 24);
        memoryStream.WriteByte(a);
        memoryStream.WriteByte(b);
        memoryStream.WriteByte(c);
        memoryStream.WriteByte(d);
    }

    public void Flush()
    {
        Write32bits();
        memoryStream.Flush();
        bits = 0; bitCount = 0;
    }

    public void Reset()
    {
        memoryStream.Position = 0;
    }

    public MemoryStream GetBuffer()
    {
        return memoryStream;
    }

    public void ResetBuffer()
    {
        memoryStream = new MemoryStream(memoryStream.Capacity);
        return;
    }
}