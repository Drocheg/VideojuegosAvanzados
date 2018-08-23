using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BitReader {
    MemoryStream memoryStream;
    long bits;
    int bitCount;

    public BitReader(MemoryStream memoryStream)
    {
        this.memoryStream = memoryStream;
    }

    private void ReadIfNecessary()
    {
        if (bitCount > 32)
        {
            return;
        }
        long a = memoryStream.ReadByte();
        long b = memoryStream.ReadByte();
        long c = memoryStream.ReadByte();
        long d = memoryStream.ReadByte();
        
        bits |= a << (bitCount + 24);
        bits |= b << (bitCount + 16);
        bits |= c << (bitCount + 8);
        bits |= d << (bitCount);
        bitCount += 32;
    }

    public bool ReadBit()
    {
        return ReadBits(1) == 1L;
    }

    public long ReadBits(int count)
    {
        if (count > 32)
        {
            throw new Exception("Max count value supported is 32");
        }
        ReadIfNecessary();
        bitCount -= count;
        long value = bits >> (bitCount);
        bits &= 0xFFFFFFFF >> (64 - bitCount);
        return value;
    }

    public int ReadInt(int min, int max)
    {
        return (int) ReadBits(32);
    }

    float ReadFloat(float min, float max, float step)
    {
        int floatBits = (int)((max - min) / step);
        long longVal = ReadBits(floatBits);
        float ret = (longVal + min) * step;
        if (ret < min || ret > max)
        {
            throw new Exception("Read a float not in between min and max.");
        }
        return ret;
    }


}
