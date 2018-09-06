﻿using System.IO;
using System;

public class BitWriter
{
    byte[] buffer;
    MemoryStream memoryStream;
    long bits;
    int bitCount;

    public BitWriter(int capacity)
    {
        this.buffer = new byte[capacity];
        this.memoryStream = new MemoryStream(buffer);
    }

    public void WriteBit(bool value)
    {
        bits |= (value ? 1L : 0L) << bitCount++;
        WriteIfNecessary();
    }

    void WriteBits(long value, int count)
    {
        if (count > 32)
        {
            throw new Exception("Max count value supported is 32");
        }
        value &= (0xFFFFFFFF >> (64 - count));
        bits |= value << bitCount;
        bitCount += count;
        WriteIfNecessary();
    }

    public void WriteInt(long value, int min, int max)
    {
        if (value < min || value > max)
        {
            throw new Exception("Value not in a valid range.");
        }
        WriteBits(value, 32);
    }

    public void WriteFloat(float value, float min, float max, float step)
    {
        if (value < min || value >= max)
        {
            throw new Exception("Value not in a valid range.");
        }
        int floatBits = (int)((max - min) / step);
        long longVal = (long)((value - min) / step);
        WriteBits(longVal, floatBits);
    }

    public void WriteString(string value) 
    {
        
    }

    private void WriteIfNecessary()
    {
        if (bitCount < 32)
        {
            return;
        }

        Write64bits();
        bits >>= 32;
        bitCount -= 32;
    }

    private void Write64bits()
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
        Write64bits();
        memoryStream.Flush();
        memoryStream.Position = 0;
        bits = 0; bitCount = 0;
    }

    public MemoryStream GetBuffer()
    {
        return memoryStream;
    }

    public void ResetBuffer()
    {
        memoryStream = new MemoryStream(buffer);
        return;
    }
}