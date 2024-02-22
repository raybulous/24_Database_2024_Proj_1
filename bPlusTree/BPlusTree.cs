using System;
using System.Collections.Generic;
using _24_Database_2024_Proj_1;
using static _24_Database_2024_Proj_1.Constants;

public class BPlusTree<TKey, TValue> where TKey : IComparable<TKey>
{
    private const int LongSize = sizeof(long);
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
        if(sibling != null){
            var newRoot = new InternalNode<TKey, TValue>();
            newRoot.Children.Add(root);
            newRoot.Children.Add(sibling);
            TKey newKey;
            while(true){
                if(sibling.Children.Count > 0 && sibling.Children[0] is InternalNode<TKey, TValue>){
                    sibling = (InternalNode<TKey, TValue>)sibling.Children[0];
                } else {
                    var leafNode = (LeafNode<TKey, TValue>)sibling.Children[0];
                    newKey = leafNode.Keys[0];
                    break;
                }
            }
            newRoot.Keys.Add(newKey);
            root = newRoot;
        }
        if (root.IsOverflow) //Check if exceed degree
        {
            var newRoot = new InternalNode<TKey, TValue>(); //new node that will point to the 2 nodes from root.Split
            newRoot.Children.Add(root);
            root.Split(newRoot, 0);
            root = newRoot; //Update root
        }
        //Console.WriteLine($"Inserted: {key}");
        //DisplayTree();
        //Console.ReadLine();
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

                // Print keys in the current node
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

    public Node<TKey, TValue> GetRoot()
    {
        return root;
    }
}
