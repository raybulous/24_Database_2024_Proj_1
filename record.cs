using System;
using System.Text;

public class Record
{
    private const int tConstLength = 10;
    private const int floatSize = sizeof(float);
    private const int intSize = sizeof(int);

    public byte[] Data { get; private set; }

    public Record(string tConst, float averageRating, int numVotes)
    {
        Data = new byte[tConstLength + floatSize + intSize];
        Encoding.ASCII.GetBytes(tConst, 0, tConst.Length, Data, 0);
        Buffer.BlockCopy(BitConverter.GetBytes(averageRating), 0, Data, tConstLength, floatSize);
        Buffer.BlockCopy(BitConverter.GetBytes(numVotes), 0, Data, tConstLength + floatSize, intSize);
    }
}
