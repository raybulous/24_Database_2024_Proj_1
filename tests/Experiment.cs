namespace _24_Database_2024_Proj_1.tests;
using System.Collections.Generic;
using System.Diagnostics;
using System;

class Experiment
{
    Disk _storage = new Disk(Constants.DiskConstants.MaxDiskSizeBytes, Constants.BlockConstants.MaxBlockSizeBytes);
    BPlusTree<int, long> _bTree = new BPlusTree<int, long>();

    int _numOfRecords;
    int _numOfRecordsInBlock;
    int _numOfBlocks;
    double _aveRating;
    int _totalRecords;
    Stopwatch _stopwatch = new Stopwatch();

    public void RunExp1()
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
                int blockNum = 0;
                string? line; // Explicitly mark as nullable if using C# 8.0 or later
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    string tConst = fields[0];
                    float averageRating = float.Parse(fields[1]);
                    int numVotes = int.Parse(fields[2]);
                    Record record = new Record(tConst, averageRating, numVotes);
                    _numOfRecords++;
                    if (!block.AddRecord(record))
                    {
                        _storage.WriteBlock(blockNum++, block);
                        block = new Block(Constants.BlockConstants.MaxBlockSizeBytes);
                        block.AddRecord(record);
                        if (_numOfRecordsInBlock == 0)
                        {
                            _numOfRecordsInBlock = _numOfRecords - 1; // - 1 cuz the last record wasn't added
                        }
                        _numOfBlocks++;
                    }
                }
                _storage.WriteBlock(blockNum++, block);
                _numOfBlocks++;
            }
            if (_numOfBlocks != _storage.BlockCount)
            {
                throw new Exception("Number of blocks does not match"); 
            }
            if (_numOfRecords != _storage.RecordCount)
            {
                throw new Exception("Number of Records does not match");
            }
            Console.WriteLine($"The number of records: {_storage.RecordCount}");
            Console.WriteLine($"The size of a record: {Constants.RecordConstants.RecordSize}");
            Console.WriteLine($"The number of records stored in a block: {_numOfRecordsInBlock}");
            Console.WriteLine($"The number of blocks for storing the data: {_storage.BlockCount}");
        }
        else
        {
            Console.WriteLine("Can't find file");
        }
        Console.WriteLine();
    }

    public void RunExp2()
    {
        Console.WriteLine("Experiment 2");
        int recordsAdded = 0;

        for (int i = 0; i < _numOfBlocks; i++)
        { //loop through datas
            Block block = _storage.ReadBlock(i);
            for (int j = 0; j < _numOfRecordsInBlock && recordsAdded < _numOfRecords; j++)
            {
                int position = j * (int)Constants.RecordConstants.RecordSize + Constants.RecordConstants.TConstLength + Constants.RecordConstants.FloatSize;
                byte[] numVotesByte = new byte[Constants.RecordConstants.IntSize];
                Buffer.BlockCopy(block.Data, position, numVotesByte, 0, Constants.RecordConstants.IntSize);
                int numOfVotes = BitConverter.ToInt32(numVotesByte, 0);
                long address = _storage.GetArrayAddress(i * Constants.BlockConstants.MaxBlockSizeBytes + j * (int)Constants.RecordConstants.RecordSize);
                _bTree.Insert(numOfVotes, address);//insert the data sequentially
                recordsAdded++;
            }
        }
        List<int> rootKeys = _bTree.GetRoot();
        // bTree.DisplayTree();
        string rootKeysString = "";
        for (int i = 0; i < rootKeys.Count; i++)
        {
            rootKeysString += rootKeys[i].ToString() + " ";
        }
        Console.WriteLine($"The parameter n of the B+ tree: {BPlusTree<int, long>.degree}");
        Console.WriteLine($"The number of nodes of the B+ tree: {_bTree.CountNodes()}");
        Console.WriteLine($"The number of levels of the B + tree: {_bTree.CountLevels()}");
        Console.WriteLine($"the content of the root node(only the keys): {rootKeysString}");
        Console.WriteLine();
    }

    public void RunExp3()
    {
        Console.WriteLine("Experiment 3");
        // Start timing the retrieval process
        _stopwatch.Restart();

        var result = _bTree.RetrieveValuesMeetingCondition(key => key == 500, 500, ref _storage);
        _stopwatch.Stop();
        List<byte[]> matchingRecords = result.matchingRecords;
        int numberOfNodesAccessed = result.numberOfNodesAccessed;

        double retrievalTime = _stopwatch.Elapsed.Microseconds;
        // Calculate the average of AvgRating
        if (matchingRecords.Count > 0)
        {
            foreach (var record in matchingRecords)
            {
                double extractedRating = Record.ExtractAverageRating(record);
                double roundedRating = Math.Round(extractedRating, 1);
                _aveRating += roundedRating;
            }
            _aveRating /= matchingRecords.Count; // Calculate the average rating
        }

        Console.WriteLine("::B+ Tree Retrieval::");
        Console.WriteLine($"Number of index nodes accessed: {numberOfNodesAccessed}");
        Console.WriteLine($"Total records found:: {matchingRecords.Count}");
        Console.WriteLine($"Average of averageRating's: {_aveRating}");
        Console.WriteLine($"Running time of the retrieval process: {retrievalTime/1000} ms");

        // Reset for brute-force linear scan
        _aveRating = 0;
        _totalRecords = 0;

        // Brute-force linear scan
        _stopwatch.Restart();
        var bruteForceResult = _storage.BruteForceScan(recordBytes =>
        {
            return Record.ExtractNumVotes(recordBytes) == 500;
        });
        _stopwatch.Stop();
        double bruteForceTime = _stopwatch.ElapsedMilliseconds;

        // Calculate the average rating from matching records using brute-force linear scan
        if (bruteForceResult.matchingRecords.Count > 0)
        {
            foreach (var recordBytes in bruteForceResult.matchingRecords)
            {
                double extractedRating = Record.ExtractAverageRating(recordBytes);
                double roundedRating = Math.Round(extractedRating, 1);
                _aveRating += roundedRating;
            }
            _aveRating /= bruteForceResult.matchingRecords.Count;
            _totalRecords = bruteForceResult.matchingRecords.Count;
        }

        // Display the statistics
        Console.WriteLine("::Brute-Force Scan::");
        Console.WriteLine($"Brute-Force Data Blocks Accessed: {bruteForceResult.blocksAccessed}");
        Console.WriteLine($"Brute-Force Scan Running Time: {bruteForceTime} ms");
        Console.WriteLine($"Average of averageRating's: {_aveRating}");
        Console.WriteLine($"Total records found: {_totalRecords}");
        Console.WriteLine();
    }

    public void RunExp4()
    {
        Console.WriteLine("Experiment 4");

        // Reset for brute-force linear scan
        _aveRating = 0.0;
        _totalRecords = 0;

        // Start timing the retrieval process for B+ Tree
        _stopwatch.Restart();
        var result = _bTree.RetrieveValuesMeetingCondition(key => key >= 30000 && key <= 40000, 30000, ref _storage);
        _stopwatch.Stop();
        List<byte[]> matchingRecords = result.matchingRecords;
        int numberOfNodesAccessed = result.numberOfNodesAccessed;

        double retrievalTime = _stopwatch.Elapsed.Microseconds;
        // Calculate the average of AvgRating
        if (matchingRecords.Count > 0)
        {
            _totalRecords = matchingRecords.Count;
            foreach (var record in matchingRecords)
            {
                double extractedRating = Record.ExtractAverageRating(record);
                double roundedRating = Math.Round(extractedRating, 1);
                _aveRating += roundedRating;
            }
            _aveRating /= matchingRecords.Count; // Calculate the average rating
        }

        Console.WriteLine("::B+ Tree Retrieval::");
        Console.WriteLine($"Number of index nodes accessed: {numberOfNodesAccessed}");
        Console.WriteLine($"Number of data blocks accessed: {matchingRecords.Count}");
        Console.WriteLine($"Average of averageRating's: {_aveRating}"); // Implement calculation
        Console.WriteLine($"Running time of the retrieval process: {retrievalTime/1000} ms");
        Console.WriteLine($"Total records found: {_totalRecords}"); // Implement calculation

        // Reset for brute-force linear scan
        _aveRating = 0.0;
        _totalRecords = 0;

        // Brute-force linear scan
        _stopwatch.Restart();
        var bruteForceResult = _storage.BruteForceScan(recordBytes =>
        {
            int numVotes = Record.ExtractNumVotes(recordBytes);
            return numVotes >= 30000 && numVotes <= 40000;
        });
        _stopwatch.Stop();
        double bruteForceTime = _stopwatch.ElapsedMilliseconds;

        // Calculate the average of AvgRating
        if (bruteForceResult.matchingRecords.Count > 0)
        {
            foreach (var recordBytes in bruteForceResult.matchingRecords)
            {
                double extractedRating = Record.ExtractAverageRating(recordBytes);
                double roundedRating = Math.Round(extractedRating, 1);
                _aveRating += roundedRating;
            }
            _aveRating /= bruteForceResult.matchingRecords.Count;
            _totalRecords = bruteForceResult.matchingRecords.Count;
        }

        Console.WriteLine();
        Console.WriteLine("::Brute-Force Scan::");
        Console.WriteLine($"Brute-Force Data Blocks Accessed: {bruteForceResult.blocksAccessed}");
        Console.WriteLine($"Brute-Force Scan Running Time: {bruteForceTime} ms");
        Console.WriteLine($"Average of averageRating's: {_aveRating}"); // Implement calculation
        Console.WriteLine($"Total records found: {_totalRecords}"); // Implement calculation
        Console.WriteLine();
    }

    public void RunExp5()
    {
        Console.WriteLine("Experiment 5");
        _stopwatch.Restart();
        int count = 0;
        bool foundKey;
        do
        {
            foundKey = _bTree.Delete(1000);
            count++;
        } while (foundKey);
        _stopwatch.Stop();
        double deletionTime = _stopwatch.ElapsedMilliseconds;

        List<int> rootKeys = _bTree.GetRoot();
        string rootKeysString = "";
        for (int i = 0; i < rootKeys.Count; i++)
        {
            rootKeysString += rootKeys[i].ToString() + " ";
        }

        Console.WriteLine("::B+ Tree Delete::");
        Console.WriteLine($"Number of records deleted: {count - 1}");
        Console.WriteLine($"The number of nodes of the B+ tree: {_bTree.CountNodes()}");
        Console.WriteLine($"The number of levels of the B + tree: {_bTree.CountLevels()}");
        Console.WriteLine($"the content of the root node(only the keys): {rootKeysString}");
        Console.WriteLine($"Deletion time on B+ tree: {deletionTime} ms");
        Console.WriteLine();

        // Brute-force linear scan

        _stopwatch.Restart();
        int blocksAccessed = _storage.BruteForceDelete(recordBytes =>
        {
            int numVotes = Record.ExtractNumVotes(recordBytes);
            return numVotes == 1000;
        });
        _stopwatch.Stop();
        double bruteForceDeleteTime = _stopwatch.ElapsedMilliseconds;
        Console.WriteLine("::Brute-Force Scan::");
        Console.WriteLine($"Brute-Force Data Blocks Accessed: {blocksAccessed}");
        Console.WriteLine($"Brute-Force Scan Running Time: {bruteForceDeleteTime} ms");
    }
}