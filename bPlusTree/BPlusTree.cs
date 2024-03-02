using System.Globalization;
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
            TKey newKey = sibling.GetFirstKey();
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
    public (List<TKey> matchingKeys, int numberOfNodesAccessed) RetrieveValuesMeetingCondition(Func<TKey, bool> condition, TKey minValue)
    {
        int numberOfNodesAccessed = 1; //root is 1 
        List<TKey> matchingKeys = new List<TKey>();
        Node<TKey, TValue> currNode = root;
        bool failedCondition = false;
        bool enteredFlag = false;
        while (!failedCondition)
        {
            if (currNode is InternalNode<TKey, TValue> internalNode)
            {
                int index = 0;
                while (index < internalNode.Keys.Count && minValue.CompareTo(internalNode.Keys[index]) > 0)
                {
                    index++;
                };
                numberOfNodesAccessed++;
                currNode = internalNode.Children[index];
            }
            else if (currNode is LeafNode<TKey, TValue> leafNode)
            {
                int index = 0;
                while (!failedCondition)
                {
                    if (index == leafNode.Keys.Count)
                    {
                        numberOfNodesAccessed++;
                        leafNode = leafNode.Next;
                        if(leafNode == null){
                            failedCondition = true;
                            break;
                        }
                        index = 0;
                    }
                    while (condition(leafNode.Keys[index]))
                    {
                        enteredFlag = true;
                        matchingKeys.Add(leafNode.Keys[index]);
                        index++;
                        if (index == leafNode.Keys.Count)
                        {
                            numberOfNodesAccessed++;
                            leafNode = leafNode.Next;
                            index = 0;
                        }
                    }
                    if (enteredFlag)
                    {
                        failedCondition = true;
                    }
                    index++;
                }
            }
        }
        return (matchingKeys, numberOfNodesAccessed);
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
                internalRoot.Keys[i] = internalRoot.Children[i + 1].GetFirstKey();
            }
        }

        return true;
    }
}
