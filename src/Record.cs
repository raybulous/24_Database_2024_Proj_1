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

    public static float ExtractAverageRating(byte[] recordBytes)
        {
            if (recordBytes == null || recordBytes.Length < tConstLength + floatSize + intSize)
            {
                throw new ArgumentException("Invalid record byte array.", nameof(recordBytes));
            }

            // Extract the bytes for averageRating, knowing its position and size
            int averageRatingOffset = tConstLength;
            return BitConverter.ToSingle(recordBytes, averageRatingOffset);
        }
    
    public static int ExtractNumVotes(byte[] recordBytes)
    {
        if (recordBytes == null || recordBytes.Length < tConstLength + floatSize + intSize)
        {
            throw new ArgumentException("Invalid record byte array.", nameof(recordBytes));
        }

        // Calculate the offset for numVotes in the recordBytes array
        int numVotesOffset = tConstLength + floatSize;

        // Extract the bytes for numVotes, knowing its position and size
        return BitConverter.ToInt32(recordBytes, numVotesOffset);
    }
}