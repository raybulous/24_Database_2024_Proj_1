using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        int diskSize = 100 * 1024 * 1024;
        int blockSize = 200;
        Disk storage = new Disk(diskSize, blockSize);

        Record record = new Record("tt0000001", 5.6f, 1645);
        /*Block block = new Block(blockSize);
        byte[] newData = new byte[blockSize];
        block.Data = newData;

        storage.writeBlock(0, block);*/
    }

    private void addData()
    {
        string filePath = "data.tsv";
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line; 
                while ((line = reader.ReadLine()) != null){
                    string[] fields = line.Split('\t');
                    string tConst = fields[0];
                    float averageRating = float.Parse(fields[1]);
                    int numVotes = int.Parse(fields[2]);
                }
            }
        }
    }
}