using System;

public class Block
{
    public byte[] Data { get; set; } 
    public Block(int blockSize)
    {
        Data = new byte[blockSize];
    }
}
