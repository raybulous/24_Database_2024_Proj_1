namespace _24_Database_2024_Proj_1;

public class Disk
{
    private byte[] disk;
    private int blockSize;

    public Disk(int diskSize, int blockSize) //Create disk of diskSize bytes
    {
        disk = new byte[diskSize];
        this.blockSize = blockSize;
    }

    public void writeBlock(int blockNum, Block block) //write block to Disk
    {
        //Array.copy(source array, source starting index, destination array, destination starting index, bytes to copy);
        Array.Copy(block.Data, 0, disk, blockNum * blockSize, blockSize);
    }

    public Block readBlock(int blockNum)
    {
        Block block = new Block(blockSize);
        Array.Copy(disk, blockNum * blockSize, block.Data, 0, blockSize);
        return block;
    }
}