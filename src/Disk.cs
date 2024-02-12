namespace _24_Database_2024_Proj_1;
using static _24_Database_2024_Proj_1.Constants;

public class Disk
{
    private byte[] _disk;
    private int _blockSize = BlockConstants.MaxBlockSizeBytes;

    private int _blockCount;
    private int _recordCount;
    public int SizeOfDiskUsed => _blockCount * _blockSize;
    private int GetIdOfLastBlock() => _blockCount > 0 ? _blockCount - 1 : -1;

    public Disk(int diskSize, int blockSize) //Create disk of diskSize bytes
    {
        _disk = new byte[diskSize];
        _blockSize = blockSize;

        _blockCount = 0;
        _recordCount = 0;
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
    }

    public int GetCurrentBlockAccess() => _recordCount;
}