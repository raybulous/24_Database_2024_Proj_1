namespace _24_Database_2024_Proj_1;

public record MovieRecord(string Tconst, double AverageRating, int NumVotes);

public class Block
{
    private const int MaxBlockSizeBytes = 200 * 1024 * 1024; // 200MB in bytes
    private const int MaxRecordsPerBlock = 100; // Example maximum number of records per block
    private List<MovieRecord> records;
    public byte[] Data { get; set; } 
    public Block(int blockSize)
    {
        Data = new byte[blockSize];
    }
}