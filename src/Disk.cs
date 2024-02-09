namespace _24_Database_2024_Proj_1;
using static _24_Database_2024_Proj_1.Constants;

public class Disk
{
    private byte[] _disk;
    private int _blockSize = BlockConstants.MaxBlockSizeBytes;
    

    public Disk(int diskSize, int blockSize) //Create disk of diskSize bytes
    {
        _disk = new byte[diskSize];
        _blockSize = blockSize;
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
}