namespace _24_Database_2024_Proj_1;

class Experiment
{
    public void runExp1()
    {
        Disk storage = new Disk(Constants.DiskConstants.MaxDiskSizeBytes, Constants.BlockConstants.MaxBlockSizeBytes);
        Block block = new Block(Constants.BlockConstants.MaxBlockSizeBytes);

        int numOfRecords = 0;
        int numOfRecordsInBlock = 0;
        int numOfBlocks = 0;

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
    }

}