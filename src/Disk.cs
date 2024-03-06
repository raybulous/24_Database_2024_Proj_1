namespace _24_Database_2024_Proj_1;
using static Constants;

public unsafe class Disk
{
    private byte[] _disk;
    private int _blockSize;
    private int _blockCount;
    private int _recordCount;
    private int _recordSize = RecordConstants.TConstLength + RecordConstants.FloatSize + RecordConstants.IntSize;

    public Disk(int diskSize, int blockSize) //Create disk of diskSize bytes
    {
        _disk = new byte[diskSize];
        _blockSize = blockSize;
        _blockCount = 0;
        _recordCount = 0;
    }

    public int RecordCount => _recordCount;
    public long GetArrayAddress(int bytePos)
    {
        fixed (byte* p = &_disk[bytePos])
        {
            return (long)p;
        }
    }

    public byte[] GetRecordFromAddress(long address)
    {
        unsafe
        {
            fixed (byte* start = &_disk[0])
            {
                byte* p = (byte*)address;
                if (p >= start && p < start + _disk.Length)
                {
                    byte[] data = new byte[_recordSize];
                    for (int i = 0; i < _recordSize; i++)
                    {
                        data[i] = *(p + i);
                    }
                    return data;
                }
            }
        }
        return null;
    }

    public int BlockCount => _blockCount;

    public void WriteBlock(int blockNum, Block block) //write block to Disk
    {
        var oldblock = ReadBlock(blockNum);
        Array.Copy(block.Data, 0, _disk, blockNum * _blockSize, _blockSize);
        if (Array.TrueForAll(oldblock.Data, b => b == 0))
        {
            _blockCount++;
        }

        int recordChange = block.CountRecords() - oldblock.CountRecords();
        _recordCount += recordChange;
    }

    public Block ReadBlock(int blockNum)
    {
        Block block = new Block(_blockSize);
        Array.Copy(_disk, blockNum * _blockSize, block.Data, 0, _blockSize);
        return block;
    }

    public (List<byte[]> matchingRecords, int blocksAccessed) BruteForceScan(Func<byte[], bool> matchesCondition)
    {
        List<byte[]> matchingRecords = new List<byte[]>();
        int numberOfBlocks = _blockCount;
        int recordsPerBlock = _blockSize / _recordSize;
        int blocksAccessed = 0;

        for (int blockIndex = 0; blockIndex < numberOfBlocks; blockIndex++)
        {
            Block block = ReadBlock(blockIndex);
            blocksAccessed++;
            for (int recordIndex = 0; recordIndex < recordsPerBlock; recordIndex++)
            {
                long position = (long)recordIndex * _recordSize;
                byte[] recordBytes = new byte[_recordSize];
                Array.Copy(block.Data, position, recordBytes, 0, _recordSize);

                if (matchesCondition(recordBytes))
                {
                    matchingRecords.Add(recordBytes);
                }
            }
        }

        return (matchingRecords, blocksAccessed);
    }

    public int BruteForceDelete(Func<byte[], bool> matchesCondition)
    {
        int numberOfBlocks = _blockCount;
        int recordsPerBlock = _blockSize / _recordSize;
        int blocksAccessed = 0;

        for (int blockIndex = 0; blockIndex < numberOfBlocks; blockIndex++)
        {
            Block block = ReadBlock(blockIndex);
            blocksAccessed++;
            for (int recordIndex = 0; recordIndex < recordsPerBlock; recordIndex++)
            {
                long position = (long)recordIndex * _recordSize;
                byte[] recordBytes = new byte[_recordSize];
                Array.Copy(block.Data, position, recordBytes, 0, _recordSize);

                if (matchesCondition(recordBytes))
                {
                    Array.Fill(recordBytes, (byte)0); //Empty record
                    Array.Copy(recordBytes, 0, block.Data, position, _recordSize); //write to block
                }
            }
            WriteBlock(blockIndex, block); //rewrite back to block;
        }
        return blocksAccessed;
    }
}