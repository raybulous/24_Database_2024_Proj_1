namespace _24_Database_2024_Proj_1;

class Experiment
{
    Disk storage = new Disk(Constants.DiskConstants.MaxDiskSizeBytes, Constants.BlockConstants.MaxBlockSizeBytes);
    BPlusTree<int, long> bTree = new BPlusTree<int, long>();

    int numOfRecords = 0;
    int numOfRecordsInBlock = 0;
    int numOfBlocks = 0;

    public void runExp1()
    {
        Console.WriteLine("Experiment 1");

        Block block = new Block(Constants.BlockConstants.MaxBlockSizeBytes);

        string currentDirectory = Directory.GetCurrentDirectory();
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
}