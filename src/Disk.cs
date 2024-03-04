namespace _24_Database_2024_Proj_1;
using static _24_Database_2024_Proj_1.Constants;

public unsafe class Disk
{
    private byte[] _disk;
    private int _blockSize = BlockConstants.MaxBlockSizeBytes;

    private int _blockCount;
    private int _recordCount;
    public int SizeOfDiskUsed => _blockCount * _blockSize;
    private int GetIdOfLastBlock() => _blockCount > 0 ? _blockCount - 1 : -1;
    private int recordSize = RecordConstants.TConstLength + RecordConstants.FloatSize + RecordConstants.IntSize;

    public Disk(int diskSize, int blockSize) //Create disk of diskSize bytes
    {
        _disk = new byte[diskSize];
        _blockSize = blockSize;

        _blockCount = 0;
        _recordCount = 0;
    }

    public long GetArrayAddress(int bytePos)
    {
        fixed (byte* p = &_disk[bytePos])
        {
            return (long)p;
        }
    }

    public List<long> GetBytePositions(List<long> addresses, long baseAddress)
    {
        List<long> relativeOffsets = addresses.Select(address => address - baseAddress).ToList();
        return relativeOffsets;
    }



    public void WriteBlock(int blockNum, Block block) //write block to Disk
    {
        //Array.copy(source array, source starting index, destination array, destination starting index, bytes to copy);
        Array.Copy(block.Data, 0, _disk, blockNum * _blockSize, _blockSize);
    }

    public Block ReadBlock(int blockNum)
    {
        Block block = new Block(_blockSize);
        Array.Copy(_disk, blockNum * _blockSize, block.Data, 0, _blockSize);
        return block;
    }

    public List<byte[]> FetchRecordsFromPositions(List<long> positions)
    {
        List<byte[]> records = new List<byte[]>();
        foreach (var position in positions)
        {
            // Ensure position is within the bounds of the disk
            if (position < 0 || position >= _disk.Length || position + recordSize > _disk.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is outside the bounds of the disk array.");
            }

            byte[] recordBytes = new byte[recordSize];
            Array.Copy(_disk, position, recordBytes, 0, recordSize);
            records.Add(recordBytes);
        }
        return records;
    }

    public List<byte[]> test(long position)
    {
        List<byte[]> records = new List<byte[]>();
        byte[] recordBytes = new byte[recordSize];
        Array.Copy(_disk, position, recordBytes, 0, recordSize);
        records.Add(recordBytes);
        return records;
    }



    public (List<byte[]> matchingRecords, int blocksAccessed) BruteForceScan(Func<byte[], bool> matchesCondition)
    {
        List<byte[]> matchingRecords = new List<byte[]>();
        int numberOfBlocks = _disk.Length / _blockSize;
        int recordsPerBlock = _blockSize / recordSize;
        int blocksAccessed = 0;

        for (int blockIndex = 0; blockIndex < numberOfBlocks; blockIndex++)
        {
            Block block = ReadBlock(blockIndex);
            blocksAccessed++;
            for (int recordIndex = 0; recordIndex < recordsPerBlock; recordIndex++)
            {
                long position = (long)recordIndex * recordSize;
                byte[] recordBytes = new byte[recordSize];
                Array.Copy(block.Data, position, recordBytes, 0, recordSize);

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
        int numberOfBlocks = _disk.Length / _blockSize;
        int recordsPerBlock = _blockSize / recordSize;
        int recordsDeleted = 0;
        int blocksAccessed = 0;

        for (int blockIndex = 0; blockIndex < numberOfBlocks; blockIndex++)
        {
            Block block = ReadBlock(blockIndex);
            blocksAccessed++;
            for (int recordIndex = 0; recordIndex < recordsPerBlock; recordIndex++)
            {
                long position = (long)recordIndex * recordSize;
                byte[] recordBytes = new byte[recordSize];
                Array.Copy(block.Data, position, recordBytes, 0, recordSize);

                if (matchesCondition(recordBytes))
                {
                    recordsDeleted++;
                    Array.Fill(recordBytes, (byte)0); //Empty record
                    Array.Copy(recordBytes, 0, block.Data, position, recordSize); //write to block
                }
            }
            WriteBlock(blockIndex, block); //rewrite back to block;
        }
        return blocksAccessed;
        //return recordsDeleted;
    }

    /*
    public Address AppendRecord(Record record)
    {
        int blockId = GetIdOfLastBlock();
        return InsertRecordAt(blockId, record);
    }

    private Address InsertRecordAt(int blockId, Record record)
    {
        Block block = null;

            if (blockId >= 0)
            {
                block = ReadBlock(blockId);
            }

            if (block == null || block.IsBlockFull)
            {
                if (_blockCount == BlockConstants.MaxBlockCount)
                {
                    throw new Exception("Not enough space on this disk");
                }

                block = new Block(_blockSize);
                WriteBlock(GetIdOfLastBlock() + 1, block);
                _blockCount++;
                blockId = GetIdOfLastBlock();
            }

            int offset = block.InsertRecord(record);
            _recordCount++;

            return new Address(blockId, offset);
    }

    public Record FetchRecord(Address address)
    {
        Block block = ReadBlock(address.BlockId);
        return block.Data[address.Offset].RecordData;
    }

    public void DeleteRecords(List<Address> addressList)
    {
        try
        {
            foreach (Address address in addressList)
            {
                Block block = ReadBlock(address.BlockId);
                bool result = block.DeleteRecordAt(address.Offset);
                _recordCount--;

                if (result)
                {
                    _blockCount--;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Record deletion unsuccessful: {e.Message}");
        }
    }

    public List<Record> RetrieveRecords(List<Address> addressList)
    {
        int blockAccess = 0;
        List<Record> recordList = new List<Record>();
        List<int> blockAccessed = new List<int>();

        try
        {
            foreach (Address address in addressList)
            {
                if (!blockAccessed.Contains(address.BlockId))
                {
                    blockAccessed.Add(address.BlockId);
                    Block block = ReadBlock(address.BlockId);
                    Record[] records = block.Data;
                    blockAccess++;
                }

                recordList.Add(FetchRecord(address));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Records retrival unsuccessful: {e.Message}");
        }

        return recordList;
    }*/

    public int GetCurrentBlockAccess() => _recordCount;
}