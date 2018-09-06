

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BitReader {
    MemoryStream memoryStream;
    ulong bits;
    int bitCount;

    public BitReader(MemoryStream memoryStream)
    {
        this.memoryStream = memoryStream;

        // Fill up bits buffer with initial 64 bits.
        FillUpTemporaryBitBuffer(0);
    }

    private void FillUpTemporaryBitBuffer(int from)
    {
        var tempBuffer = new byte[4];
        int readBytes = memoryStream.Read(tempBuffer, 0, tempBuffer.Length);
        if (readBytes < 0) {
            return;
        }

        for (int i = 0; i < readBytes; i++){
            ulong t = tempBuffer[i];
            bits |=  t << (i * 8); 
        }
        
    }

    private void ReadIfNecessary()
    {
        if (bitCount >= 32)
        {
            // Fill up all 64 bits of the bits variable.
            var filledUpBytes = bitCount / 8;
            bits >>= filledUpBytes * 8;
            bitCount %= 8;
            FillUpTemporaryBitBuffer(8 - filledUpBytes);
            return;
        }
    }

    public bool ReadBit()
    {
        return ReadBits(1) == 1L;
    }

    public ulong ReadBits(int count)
    {
        if (count > 32)
        {
            throw new Exception("Max count value supported is 32");
        }
        ReadIfNecessary();
        var from = bitCount;
        var to = bitCount + count;
        var mask = (ulong.MaxValue << from) & (ulong.MaxValue >> (64 - to) );
        bitCount = to;
        return (bits & mask) >> from;
    }

    public int ReadInt(int min, int max)
    {
        return (int) ReadBits(32);
    }

    float ReadFloat(float min, float max, float step)
    {
        int floatBits = (int)((max - min) / step);
        ulong longVal = ReadBits(floatBits);
        float ret = (longVal + min) * step;
        if (ret < min || ret > max)
        {
            throw new Exception("Read a float not in between min and max.");
        }
        return ret;
    }

    public MemoryStream GetBuffer()
    {
        return memoryStream;
    }

    public void ResetBuffer()
    {
        memoryStream = new MemoryStream();
        return;
    }
}
