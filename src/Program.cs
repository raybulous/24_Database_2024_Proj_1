using _24_Database_2024_Proj_1.utils;
using static _24_Database_2024_Proj_1.Constants;

namespace _24_Database_2024_Proj_1;
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Experiment experiment = new Experiment(); // Create an instance of the Experiment class

        // Call the runExp1 and runExp2 methods
        experiment.runExp1();
        Console.WriteLine();
        experiment.runExp2();
        Console.WriteLine();
        experiment.runExp3();
        Console.WriteLine();
        experiment.runExp4();
        Console.WriteLine();

        // Disk storage = new Disk(DiskConstants.MaxDiskSizeBytes, BlockConstants.MaxBlockSizeBytes);
        // Block block = new Block(BlockConstants.MaxBlockSizeBytes);

        // //Sample//
        // Record record = new Record("tt0000001", 5.6f, 1645); // Actual data will read from data.tsv
        // Console.WriteLine($"Max Available Block Size: {block.GetAvailableSpace()} Bytes");
        // Console.WriteLine($"Max Available Block Slots: {block.GetAvailableSlots()}");
        // Console.WriteLine($"Max Available Block Slots (Reserved): {block.GetAvailableReservedSlots()}");
        
        // Console.WriteLine($"Record Size: {Utils.CalculateRecordSize(record)} Bytes");
        // if (block.AddRecord(record))
        // {
        //     Console.WriteLine();
        //     Console.WriteLine("Write Success");
        //     Console.WriteLine();
        //     Console.WriteLine($"Max Available Block Size: {block.GetAvailableSpace()} Bytes");
        //     Console.WriteLine($"Max Reserved Available Block Space: {block.GetAvailableReservedSpace()}");
        //     Console.WriteLine($"Max Available Block Slots: {block.GetAvailableSlots()}");
        //     Console.WriteLine($"Max Available Block Slots (Reserved): {block.GetAvailableReservedSlots()}");
        // }
        // else
        // {
        //     Console.WriteLine();
        //     Console.WriteLine("Write Failed");
        //     Console.WriteLine();
        //     Console.WriteLine($"Max Available Block Size: {block.GetAvailableSpace()} Bytes");
        //     Console.WriteLine($"Max Reserved Available Block Space: {block.GetAvailableReservedSpace()}");
        //     Console.WriteLine($"Max Available Block Slots: {block.GetAvailableSlots()}");
        //     Console.WriteLine($"Max Available Block Slots (Reserved): {block.GetAvailableReservedSlots()}");
        // }
        // // byte[] newData = new byte[BlockConstants.MaxBlockSizeBytes];
        // // block.Data = newData;

        // storage.WriteBlock(0, block);
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


