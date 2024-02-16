class Experiment2
{
    public void runExperiment()
    {
        Console.WriteLine("Experiment 2");
        BPlusTree<int, long> bTree = new BPlusTree<int, long>();
        while ()
        { //loop through datas
            bTree.Insert();//insert the data sequentially
        }
        Console.WriteLine($"The parameter n of the B+ tree: {BPlusTree<int, long>.degree}");
        Console.WriteLine($"The number of nodes of the B+ tree: {bTree.CountNodes()}");
        Console.WriteLine($"The number of levels of the B + tree: {bTree.CountLevels()}");
        Console.WriteLine($"the content of the root node(only the keys): {bTree.GetRoot()}");
    }
}