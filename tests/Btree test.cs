using System;
using _24_Database_2024_Proj_1;

class BTreeTest
{
    public static void test1()
    {
        // Initialize your BPlusTree with an appropriate degree
        BPlusTree<int, string> bPlusTree = new BPlusTree<int, string>(); // Example degree

        // Define an array of keys to insert
        int[] keysToInsert = { 1, 2, 3, 4, 5 };
        string[] valuesToInsert = { "a", "b", "c", "d", "e" };

        // Insert some key-value pairs and verify each insertion
        Console.WriteLine("Inserting key-value pairs...");
        for (int i = 0; i < keysToInsert.Length; i++)
        {
            bPlusTree.Insert(keysToInsert[i], valuesToInsert[i]);
            // Verify insertion
            var searchResult = bPlusTree.Search(keysToInsert[i]);
            Console.WriteLine($"Inserted key {keysToInsert[i]}, search result: {(searchResult == null ? "Not Found" : searchResult)}");
        }

        // Deleting a key (e.g., 3) and verifying
        DeleteAndVerify(bPlusTree, 3);
        
        Console.WriteLine();

        // Inserting another key-value pair (6, 'f') and verifying
        Console.WriteLine("Inserting another key-value pair (6, 'f')...");
        bPlusTree.Insert(6, "f");
        // Verify insertion
        var verifyInsert = bPlusTree.Search(6);
        Console.WriteLine($"Inserted key 6, search result: {(verifyInsert == null ? "Not Found" : verifyInsert)}");

        // Deleting another key (2) and verifying
        DeleteAndVerify(bPlusTree, 2);
        
        Console.WriteLine();

        // Optionally, print the B-tree structure or contents here if you have such a method

        // Add any additional operations or checks you want to perform
    }

    private static void DeleteAndVerify(BPlusTree<int, string> tree, int key)
    {
        Console.WriteLine($"Deleting key ({key})...");
        bool deleteResult = tree.Delete(key);
        Console.WriteLine($"Delete operation successful: {deleteResult}");

        // Search for the key to verify deletion
        Console.WriteLine($"Searching for the deleted key ({key})...");
        var searchResult = tree.Search(key);
        Console.WriteLine($"Search result for key {key}: {(searchResult == null ? "Not Found" : searchResult)}");
    }
}
