

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BitReader {
    MemoryStream memoryStream;
    ulong bits;
    int bitCount;
	int bitsBufferLimit;

    public BitReader(MemoryStream memoryStream)
    {
        this.memoryStream = memoryStream;

        // Fill up bits buffer with initial 64 bits.
		bitsBufferLimit = 32;
        FillUpTemporaryBitBuffer(0, 4);
    }

    private void FillUpTemporaryBitBuffer(int from, int bytes)
    {

        var tempBuffer = new byte[bytes];
        int readBytes = memoryStream.Read(tempBuffer, 0, tempBuffer.Length);
        if (readBytes < 0) {
            return;
        }

        for (int i = 0; i < readBytes; i++){
            ulong t = tempBuffer[i];
            bits |=  t << ((i * 8) + from);
        }

    }

    private void ReadIfNecessary(int required)
    {
        if (bitsBufferLimit - bitCount < required)
        {
            // Fill up 32 bits of the bits variable.
            var filledUpBytes = bitCount / 8;
            bits >>= filledUpBytes * 8;
			var requiredNewBytes = (required - (bitsBufferLimit - bitCount) -1 ) / 8 + 1;
			var shiftedOldBitBufferLimit = bitsBufferLimit - filledUpBytes * 8;
			bitsBufferLimit = shiftedOldBitBufferLimit + requiredNewBytes * 8;
            bitCount %= 8;
            FillUpTemporaryBitBuffer(shiftedOldBitBufferLimit, requiredNewBytes);
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
        ReadIfNecessary(count);
        var from = bitCount;
        var to = bitCount + count;
        var mask = (ulong.MaxValue << from) & (ulong.MaxValue >> (64 - to) );
        bitCount = to;
        return (bits & mask) >> from;
    }

    public int ReadInt(int min, int max)
    {
		// 
        return (int) ReadBits((int) Math.Log((double) (max - min), 2.0) + 1);
    }

    public float ReadFloat(float _min, float _max, float _step)
    {
        int floatBits = (int) (Math.Log((_max - _min) / _step, 2.0) + 1);
        ulong longVal = ReadBits(floatBits);
        float ret = longVal * _step + _min;
//        Debug.Log("min: " + _min + "max: " + _max + "step: " + _step + "bits: " + floatBits + "logVall: " + longVal + "ret: " + ret);
        // if (ret < _min || ret > _max)
        // {
        //     throw new Exception("Read a float not in between min and max.");
        // }
        return ret;
    }

    public string ReadString(int lenght)
    {
        char[] chars = new char[lenght];
        for (int i = 0; i < lenght; i++)
        {
            chars[i] = (char) ReadBits(8);
        }

        return new string(chars);
    }

    public MemoryStream GetBuffer()
    {
        return memoryStream;
    }

    public bool HasNext()
    {
        return memoryStream.Position < memoryStream.Length;
    }

    public void ResetBuffer()
    {
        memoryStream = new MemoryStream();
        return;
    }
}
