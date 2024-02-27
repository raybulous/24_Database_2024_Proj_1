using System.Text;

namespace _24_Database_2024_Proj_1;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Runtime.CompilerServices;

class Experiment
{
    Disk storage = new Disk(Constants.DiskConstants.MaxDiskSizeBytes, Constants.BlockConstants.MaxBlockSizeBytes);
    BPlusTree<int, long> bTree = new BPlusTree<int, long>();

    int numOfRecords = 0;
    int numOfRecordsInBlock = 0;
    int numOfBlocks = 0;
    double aveRating = 0;
    int totalRecords = 0;
    Stopwatch stopwatch = new Stopwatch();

    public void runExp1()
    {
        Console.WriteLine("Experiment 1");

        Block block = new Block(Constants.BlockConstants.MaxBlockSizeBytes);

        //string currentDirectory = Directory.GetCurrentDirectory();
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(currentDirectory, "..", "..", "..", "data.tsv");
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                reader.ReadLine(); //Read first line to skip headers
                string line;
                int blockNum = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    string tConst = fields[0];
                    float averageRating = float.Parse(fields[1]);
                    int numVotes = int.Parse(fields[2]);
                    Record record = new Record(tConst, averageRating, numVotes);
                    numOfRecords++;
                    if (!block.AddRecord(record))
                    {
                        storage.WriteBlock(blockNum++, block);
                        block = new Block(Constants.BlockConstants.MaxBlockSizeBytes);
                        block.AddRecord(record);
                        if (numOfRecordsInBlock == 0)
                        {
                            numOfRecordsInBlock = numOfRecords - 1; // - 1 cuz the last record wasn't added
                        }
                        numOfBlocks++;
                    }
                }
                storage.WriteBlock(blockNum++, block);
                numOfBlocks++;
            }
            Console.WriteLine($"The number of records: {numOfRecords}");
            Console.WriteLine($"The size of a record: {Constants.RecordConstants.RecordSize}");
            Console.WriteLine($"The number of records stored in a block: {numOfRecordsInBlock}");
            Console.WriteLine($"The number of blocks for storing the data: {numOfBlocks}");
        }
        else
        {
            Console.WriteLine("Can't find file");
        }
        Console.WriteLine();
    }

    public void runExp2()
    {
        Console.WriteLine("Experiment 2");
        int recordsAdded = 0;


        for (int i = 0; i < numOfBlocks; i++)
        { //loop through datas
            Block block = storage.ReadBlock(i);
            for (int j = 0; j < numOfRecordsInBlock && recordsAdded < numOfRecords; j++ )
            {
                int position = j * (int)Constants.RecordConstants.RecordSize + Constants.RecordConstants.TConstLength + Constants.RecordConstants.FloatSize;
                byte[] numVotesByte = new byte[Constants.RecordConstants.IntSize]; 
                Buffer.BlockCopy(block.Data, position, numVotesByte, 0, Constants.RecordConstants.IntSize);
                int numOfVotes = BitConverter.ToInt32(numVotesByte, 0);
                long address = storage.GetArrayAddress(i * Constants.BlockConstants.MaxBlockSizeBytes + j * (int)Constants.RecordConstants.RecordSize);
                bTree.Insert(numOfVotes, address);//insert the data sequentially
                recordsAdded++;
            }
        }
        Console.WriteLine($"The parameter n of the B+ tree: {BPlusTree<int, long>.degree}");
        Console.WriteLine($"The number of nodes of the B+ tree: {bTree.CountNodes()}");
        Console.WriteLine($"The number of levels of the B + tree: {bTree.CountLevels()}");
        Console.WriteLine($"the content of the root node(only the keys): {bTree.GetRoot()}");
        Console.WriteLine();
    }

    public void runExp3()
    {
        Console.WriteLine("Experiment 3");
        // Start timing the retrieval process
        stopwatch.Restart();
        List<long> recordPositions = bTree.RetrieveValuesMeetingCondition(key => key == 500);
        stopwatch.Stop();
        long retrievalTime = stopwatch.ElapsedMilliseconds;
        long baseAddress = storage.GetArrayAddress(0); 
        // Calculate the average of AvgRating
        if (recordPositions.Count > 0)
        {
            List<long> addressList = storage.GetBytePositions(recordPositions,baseAddress);
            var records = storage.FetchRecordsFromPositions(addressList);
            foreach (var recordBytes in records)
            {
                double extractedRating = Record.ExtractAverageRating(recordBytes);
                double roundedRating = Math.Round(extractedRating, 1);
                aveRating += roundedRating;
            }
            aveRating /= records.Count; // Calculate the average rating
        }
        
        Console.WriteLine("::B+ Tree Retrieval::");
        Console.WriteLine($"Number of index nodes accessed: {bTree.CountIndexNodesAccessed(key => key == 500)}");
        Console.WriteLine($"Total records found:: {recordPositions.Count}");
        Console.WriteLine($"Average of averageRating's: {aveRating}");
        Console.WriteLine($"Running time of the retrieval process: {retrievalTime} ms");

        // Reset for brute-force linear scan
        aveRating = 0;
        totalRecords = 0;

        // Brute-force linear scan
        stopwatch.Restart();
        var matchingRecords = storage.BruteForceScan(recordBytes =>
        {
            return Record.ExtractNumVotes(recordBytes) == 500;
        });
        stopwatch.Stop();
        long bruteForceTime = stopwatch.ElapsedMilliseconds;

        // Calculate the average rating from matching records using brute-force linear scan
        if (matchingRecords.Count > 0)
        {
            foreach (var recordBytes in matchingRecords)
            {
                double extractedRating = Record.ExtractAverageRating(recordBytes);
                double roundedRating = Math.Round(extractedRating, 1);
                aveRating += roundedRating;
            }
            aveRating /= matchingRecords.Count;
            totalRecords = matchingRecords.Count;
        }

        // Assuming each block is fully utilized for simplicity
        int bruteForceDataBlocksAccessed = matchingRecords.Count > 0 ? (int)Math.Ceiling((double)matchingRecords.Count / Constants.BlockConstants.MaxRecordsPerBlock) : 0;

        // Display the statistics
        Console.WriteLine("::Brute-Force Scan::");
        Console.WriteLine($"Brute-Force Data Blocks Accessed: {bruteForceDataBlocksAccessed}");
        Console.WriteLine($"Brute-Force Scan Running Time: {bruteForceTime} ms");
        Console.WriteLine($"Average of averageRating's: {aveRating}");
        Console.WriteLine($"Total records found: {totalRecords}");
    }

    public void runExp4()
    {
    Console.WriteLine("Experiment 4");

    // Reset for brute-force linear scan
    aveRating = 0.0;
    totalRecords = 0;

    // Start timing the retrieval process for B+ Tree
    stopwatch.Restart();
    var recordPositions = bTree.RetrieveValuesMeetingCondition(key => key >= 30000 && key <= 40000);
    stopwatch.Stop();
    long retrievalTime = stopwatch.ElapsedMilliseconds;
    long baseAddress = storage.GetArrayAddress(0); 
    // Calculate the average of AvgRating
    if (recordPositions.Count > 0)
    {
        totalRecords = recordPositions.Count;
        List<long> addressList = storage.GetBytePositions(recordPositions,baseAddress);
        var records = storage.FetchRecordsFromPositions(addressList);
        foreach (var recordBytes in records)
        {
            double extractedRating = Record.ExtractAverageRating(recordBytes);
            double roundedRating = Math.Round(extractedRating, 1);
            aveRating += roundedRating;
        }
        aveRating /= records.Count; // Calculate the average rating
    }

    Console.WriteLine("::B+ Tree Retrieval::");
    Console.WriteLine($"Number of index nodes accessed: {bTree.CountIndexNodesAccessed(key => key >= 30000 && key <= 40000)}");
    Console.WriteLine($"Number of data blocks accessed: {recordPositions.Count}");
    Console.WriteLine($"Average of averageRating's: {aveRating}"); // Implement calculation
    Console.WriteLine($"Running time of the retrieval process: {retrievalTime} ms");
    Console.WriteLine($"Total records found: {totalRecords}"); // Implement calculation

    // Reset for brute-force linear scan
    aveRating = 0.0;
    totalRecords = 0;

    // Brute-force linear scan
    stopwatch.Restart();
    var matchingRecords = storage.BruteForceScan(recordBytes =>
    {
        int numVotes = Record.ExtractNumVotes(recordBytes);
        return numVotes >= 30000 && numVotes <= 40000;
    });
    stopwatch.Stop();
    long bruteForceTime = stopwatch.ElapsedMilliseconds;
    
    // Calculate the average of AvgRating
    if (matchingRecords.Count > 0)
    {
        foreach (var recordBytes in matchingRecords)
        {
            double extractedRating = Record.ExtractAverageRating(recordBytes);
            double roundedRating = Math.Round(extractedRating, 1);
            aveRating += roundedRating;
        }
        aveRating /= matchingRecords.Count;
        totalRecords = matchingRecords.Count;
    }

    // Assuming each block is fully utilized for simplicity, calculate average rating and blocks accessed
    int bruteForceDataBlocksAccessed = matchingRecords.Count > 0 ? (int)Math.Ceiling((double)matchingRecords.Count / Constants.BlockConstants.MaxRecordsPerBlock) : 0;

    Console.WriteLine("::Brute-Force Scan::");
    Console.WriteLine($"Brute-Force Data Blocks Accessed: {bruteForceDataBlocksAccessed}");
    Console.WriteLine($"Brute-Force Scan Running Time: {bruteForceTime} ms");
    Console.WriteLine($"Average of averageRating's: {aveRating}"); // Implement calculation
    Console.WriteLine($"Total records found: {totalRecords}"); // Implement calculation
    }

    public void runExp5()
    {
    Console.WriteLine("Experiment 5");

    // Reset for B+ Tree update
    bTree.Clear();

    // Start timing the update process
    stopwatch.Restart();
    
    // Delete records with "numVotes" equal to 1000
    List<long> recordsToDelete = bTree.RetrieveValuesMeetingCondition(key => key == 1000);
    foreach (var recordPosition in recordsToDelete)
    {
        storage.DeleteRecord(recordPosition);
    }

    // Rebuild the B+ Tree after deletion
    for (int i = 0; i < numOfBlocks; i++)
    {
        Block block = storage.ReadBlock(i);
        for (int j = 0; j < numOfRecordsInBlock; j++)
        {
            int position = j * (int)Constants.RecordConstants.RecordSize + Constants.RecordConstants.TConstLength + Constants.RecordConstants.FloatSize;
            byte[] numVotesByte = new byte[Constants.RecordConstants.IntSize];
            Buffer.BlockCopy(block.Data, position, numVotesByte, 0, Constants.RecordConstants.IntSize);
            int numOfVotes = BitConverter.ToInt32(numVotesByte, 0);
            long address = storage.GetArrayAddress(i * Constants.BlockConstants.MaxBlockSizeBytes + j * (int)Constants.RecordConstants.RecordSize);
            bTree.Insert(numOfVotes, address);
        }
    }
    
    stopwatch.Stop();
    long updateProcessTime = stopwatch.ElapsedMilliseconds;

    // Display updated B+ Tree statistics
    Console.WriteLine("::Updated B+ Tree::");
    Console.WriteLine($"Number nodes of the updated B+ tree: {bTree.CountNodes()}");
    Console.WriteLine($"Number of levels of the updated B+ tree: {bTree.CountLevels()}");
    Console.WriteLine($"Content of the root node of the updated B+ tree (only the keys): {bTree.GetRoot()}");

    // Reset for brute-force linear scan
    aveRating = 0.0;
    totalRecords = 0;

    // Brute-force linear scan
    stopwatch.Restart();
    var matchingRecords = storage.BruteForceScan(recordBytes =>
    {
        return Record.ExtractNumVotes(recordBytes) == 1000;
    });
    stopwatch.Stop();
    long bruteForceTime = stopwatch.ElapsedMilliseconds;

    // Calculate the average of AvgRating
    if (matchingRecords.Count > 0)
    {
        foreach (var recordBytes in matchingRecords)
        {
            double extractedRating = Record.ExtractAverageRating(recordBytes);
            double roundedRating = Math.Round(extractedRating, 1);
            aveRating += roundedRating;
        }
        aveRating /= matchingRecords.Count;
        totalRecords = matchingRecords.Count;
    }

    // Assuming each block is fully utilized for simplicity
    int bruteForceDataBlocksAccessed = matchingRecords.Count > 0 ? (int)Math.Ceiling((double)matchingRecords.Count / Constants.BlockConstants.MaxRecordsPerBlock) : 0;

    // Display statistics for brute-force linear scan after deletion
    Console.WriteLine("::Brute-Force Scan After Deletion::");
    Console.WriteLine($"Brute-Force Data Blocks Accessed: {bruteForceDataBlocksAccessed}");
    Console.WriteLine($"Brute-Force Scan Running Time: {bruteForceTime} ms");
    Console.WriteLine($"Average of averageRating's: {aveRating}");
    Console.WriteLine($"Total records found: {totalRecords}");
    Console.WriteLine($"Running time of the update process: {updateProcessTime} ms");
    Console.WriteLine();
    }
}