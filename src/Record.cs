using System.Text;

namespace _24_Database_2024_Proj_1;
using static Constants;

public class Record
{
    private const int tConstLength = RecordConstants.TConstLength;
    private const int floatSize = RecordConstants.FloatSize;
    private const int intSize = RecordConstants.IntSize;

    public byte[] Data { get; private set; }

    public Record(string tConst, float averageRating, int numVotes)
    {
        Data = new byte[tConstLength + floatSize + intSize];
        Encoding.ASCII.GetBytes(tConst, 0, tConst.Length, Data, 0);
        Buffer.BlockCopy(BitConverter.GetBytes(averageRating), 0, Data, tConstLength, floatSize);
        Buffer.BlockCopy(BitConverter.GetBytes(numVotes), 0, Data, tConstLength + floatSize, intSize);
    }
}