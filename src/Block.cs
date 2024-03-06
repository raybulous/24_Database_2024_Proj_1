namespace _24_Database_2024_Proj_1;
using static _24_Database_2024_Proj_1.Constants;
using static _24_Database_2024_Proj_1.utils.Utils;

public class Block
{
    private int _maxBlockSizeBytes = BlockConstants.MaxBlockSizeBytes;
    private static readonly double MaxReservedBlockSizeBytes = MaxReserveSizeCalculator();
    private static readonly int MaxReserveRecordsPerBlock = MaxReserveRecordCalculator(RecordConstants.RecordSize);
    private static readonly int MaxRecordsPerBlock = MaxRecordCalculator(RecordConstants.RecordSize);
    private List<Record> _records;
    public byte[] Data { get; private set; }
    public Block(int blockSize)
    {
        if (blockSize <= 0 || blockSize > BlockConstants.MaxBlockSizeBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(blockSize), "Block size must be positive and within allowed limits.");
        }
        Data = new byte[blockSize];
        _records = new List<Record>();
    }

    // Method to add a record (implement serialization as needed)

    public bool AddRecord(Record record)
    {
        if (_records.Count >= MaxRecordsPerBlock)
        {
            return false;
        }

        _records.Add(record);

        int position = _records.Count * (int)CalculateRecordSize(record) - (int)CalculateRecordSize(record);
        Buffer.BlockCopy(record.Data, 0, Data, position, (int)CalculateRecordSize(record));
        return true;
    }

    public int GetAvailableSlots()
    {
        return MaxRecordsPerBlock - _records.Count;
    }

    public int GetAvailableReservedSlots()
    {
        return MaxReserveRecordsPerBlock - _records.Count;
    }

    public double GetAvailableSpace()
    {
        int lengthOfList = _records.Count;
        double listSize = lengthOfList * RecordConstants.RecordSize;
        return _maxBlockSizeBytes - listSize;
    }

    public double GetAvailableReservedSpace()
    {
        return MaxReservedBlockSizeBytes - _records.Count * RecordConstants.RecordSize;
    }

    public int CountRecords()
    {
        int recordsCount = 0;
        for (int i = 0; i < Data.Length; i += (int)Constants.RecordConstants.RecordSize)
        {
            bool isRecord = false;
            for (int j = 0; j < (int)Constants.RecordConstants.RecordSize && i + j < Constants.BlockConstants.MaxBlockSizeBytes; j++)
            {
                if (Data[i + j] != 0)
                {
                    isRecord = true;
                    break;
                }
            }
            if (isRecord)
            {
                recordsCount++;
            }
        }
        return recordsCount;
    }
}