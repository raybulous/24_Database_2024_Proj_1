using static _24_Database_2024_Proj_1.Constants;

public class BPlusTree<TKey, TValue> where TKey : IComparable<TKey>
{
    private const int LongSize = sizeof(long);
    //Calculate the maximum degree for the max block size
    public static int degree = (BlockConstants.MaxBlockSizeBytes - LongSize) / (LongSize + RecordConstants.IntSize); 
    private Node<TKey, TValue> root;

    public BPlusTree()
    {
        //Initialise first node 
        this.root = new LeafNode<TKey, TValue>();
    }

    public void Insert(TKey key, TValue value)
    {
        var sibling = root.Insert(key, value);
        //if insert results in splitting, create a newRoot and add the original root and sibling as children
        if (sibling != null)
        {
            var newRoot = new InternalNode<TKey, TValue>();
            newRoot.Children.Add(root);
            newRoot.Children.Add(sibling);
            TKey newKey = newRoot.GetFirstKey();
            newRoot.Keys.Add(newKey);
            root = newRoot;
        }
        if (root.IsOverflow) //Check for overflow to split 
        {
            var newRoot = new InternalNode<TKey, TValue>(); //new node that will point to the 2 nodes from root.Split
            newRoot.Children.Add(root);
            root.Split(newRoot, 0);
            root = newRoot; //Update root
        }
        // Console.WriteLine($"Inserted: {key}");
        // DisplayTree();
        // Console.ReadLine();
    }

    public void DisplayTree()
    {
        Queue<Node<TKey, TValue>> queue = new Queue<Node<TKey, TValue>>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            int levelNodeCount = queue.Count;
            for (int i = 0; i < levelNodeCount; i++)
            {
                Node<TKey, TValue> currentNode = queue.Dequeue();

                if (currentNode is InternalNode<TKey, TValue> internalNode)
                {
                    foreach (var child in internalNode.Children)
                    {
                        queue.Enqueue(child);
                    }
                }

                Console.Write("[");
                for (int j = 0; j < currentNode.Keys.Count; j++)
                {
                    Console.Write($"{currentNode.Keys[j]}");
                    if (j < currentNode.Keys.Count - 1)
                        Console.Write(", ");
                }
                Console.Write("] ");
            }
            Console.WriteLine();
        }
    }

    public TValue Search(TKey key)
    {
        return root.Search(key);
    }

    public int CountNodes()
    {
        int nodeCount = 0;
        Queue<Node<TKey, TValue>> queue = new Queue<Node<TKey, TValue>>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            int levelNodeCount = queue.Count;
            for (int i = 0; i < levelNodeCount; i++)
            {
                Node<TKey, TValue> currentNode = queue.Dequeue();
                nodeCount++;

                if (currentNode is InternalNode<TKey, TValue> internalNode)
                {
                    foreach (var child in internalNode.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        return nodeCount;
    }

    public int CountLevels()
    {
        if (root == null)
            return 0;
        int levels = 0;
        Queue<Node<TKey, TValue>> queue = new Queue<Node<TKey, TValue>>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            int levelNodeCount = queue.Count;
            for (int i = 0; i < levelNodeCount; i++)
            {
                Node<TKey, TValue> currentNode = queue.Dequeue();

                if (currentNode is InternalNode<TKey, TValue> internalNode)
                {
                    foreach (var child in internalNode.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            levels++;
        }

        return levels;
    }

    public List<TKey> GetRoot()
    {
        return root.Keys;
    }

    // Method to retrieve values that meet a specific condition on keys
    public List<TValue> RetrieveValuesMeetingCondition(Func<TKey, bool> condition)
    {
        List<TValue> matchingValues = new List<TValue>();
        TraverseAndCollect(root, condition, matchingValues);
        return matchingValues;
    }

    // Helper method to traverse the tree and collect values
    private void TraverseAndCollect(Node<TKey, TValue> node, Func<TKey, bool> condition, List<TValue> matchingValues)
    {
        if (node is LeafNode<TKey, TValue> leaf)
        {
            // Leaf node, check each key against the condition
            for (int i = 0; i < leaf.Keys.Count; i++)
            {
                if (condition(leaf.Keys[i]))
                {
                    matchingValues.Add(leaf.Values[i]);
                }
            }
        }
        else if (node is InternalNode<TKey, TValue> internalNode)
        {
            // Internal node, recursively traverse child nodes
            foreach (var child in internalNode.Children)
            {
                TraverseAndCollect(child, condition, matchingValues);
            }
        }
    }

    public int CountIndexNodesAccessed(Func<TKey, bool> condition)
    {
        int count = 0;
        CountNodes(root, condition, ref count);
        return count;
    }

    public bool Delete(TKey key)
    {
        //Traverse the tree and delete the first instance of the key from the leaf node.
        bool isDeleted = root.Delete(key);
        if (!isDeleted)
        {
            return false; // Key not found.
        }

        //Root node is left with 1 child, that child is set as root instead
        if (root is InternalNode<TKey, TValue> && root.Keys.Count == 0) 
        {
            // Promote the single child as the new root if root is empty.
            root = ((InternalNode<TKey, TValue>)root).Children[0];
        }
        //Update keys
        if (root is InternalNode<TKey, TValue> internalRoot)
        {
            for (int i = 0; i < internalRoot.Keys.Count; i++)
            {
                internalRoot.Keys[i] = internalRoot.Children[i+1].GetFirstKey();
            }
        }

        return true;
    }

    // Helper method to count index nodes accessed during traversal
    private void CountNodes(Node<TKey, TValue> node, Func<TKey, bool> condition, ref int count)
    {
        if (node is InternalNode<TKey, TValue>)
        {
            count++; // Counting this node as accessed
            InternalNode<TKey, TValue> internalNode = (InternalNode<TKey, TValue>)node;
            foreach (var child in internalNode.Children)
            {
                CountNodes(child, condition, ref count);
            }
        }
        else if (node is LeafNode<TKey, TValue> leaf)
        {
            // Optionally, count leaf nodes if they match the condition
            if (leaf.Keys.Exists(k => condition(k)))
            {
                count++;
            }
        }
    }
}
