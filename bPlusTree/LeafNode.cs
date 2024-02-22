public class LeafNode<TKey, TValue> : Node<TKey, TValue> where TKey : IComparable<TKey>
{
    public List<TValue> Values { get; private set; }
    public LeafNode<TKey, TValue> Next { get; set; }

    public LeafNode()
    {
        Keys = new List<TKey>();
        Values = new List<TValue>();
        Next = null;
    }

    public override bool IsOverflow { get { return Keys.Count > BPlusTree<TKey, TValue>.degree; } }

    public override InternalNode<TKey, TValue> Insert(TKey key, TValue value)
    {
        int index = 0;
        while (index < Keys.Count && key.CompareTo(Keys[index]) >= 0)
        {
            index++;
        }
        Keys.Insert(index, key);
        Values.Insert(index, value);
        return null;
    }

    public override TValue Search(TKey key)
    {
        int index = Keys.BinarySearch(key);
        if (index >= 0)
            return Values[index];
        else
            return default(TValue);
    }

    public override InternalNode<TKey, TValue> Split(InternalNode<TKey, TValue> parent, int index)
    {
        var newNode = new LeafNode<TKey, TValue>();
        int degree = BPlusTree<TKey, TValue>.degree;
        int count = Keys.Count;
        newNode.Keys.AddRange(Keys.GetRange(count - degree / 2, degree / 2));
        newNode.Values.AddRange(Values.GetRange(count - degree / 2, degree / 2));
        Keys.RemoveRange(count - degree / 2, degree / 2);
        Values.RemoveRange(count - degree / 2, degree / 2);
        parent.Children.Insert(index + 1, newNode);
        newNode.Next = Next;
        Next = newNode;
        if (parent.Keys.Count != degree || index == 0 || index == degree - 1)
        {
            parent.Keys.Insert(index, newNode.Keys[0]);
        }
        else
        {
            var parent2 = new InternalNode<TKey, TValue>();
            parent2.Keys.AddRange(parent.Keys.GetRange(degree / 2, degree / 2));
            parent2.Children.AddRange(parent.Children.GetRange((degree + 2) / 2, (degree + 2) / 2));
            parent.Keys.RemoveRange(degree / 2, degree / 2);
            parent.Children.RemoveRange((degree + 2) / 2, (degree + 2) / 2);
            
                for (int i = 0; i < parent.Keys.Count; i++)
                {
                    var newKey = FindKeyValue(parent.Children[i+1]);
                    parent.Keys[i] = newKey;
                }
                for (int i = 0; i < parent2.Keys.Count; i++)
                {
                    var newKey = FindKeyValue(parent2.Children[i+1]);
                    parent2.Keys[i] = newKey;
                }
            
            return parent2;
        }
        return null;
    }

    private TKey FindKeyValue(Node<TKey, TValue> inputNode)
    {
        TKey key;
        if (inputNode is InternalNode<TKey, TValue> node)
        {
            while (true)
            {
                if (node.Children.Count > 0 && node.Children[0] is InternalNode<TKey, TValue>)
                {
                    node = (InternalNode<TKey, TValue>)node.Children[0];
                }
                else
                {
                    var leafNode = (LeafNode<TKey, TValue>)node.Children[0];
                    key = leafNode.Keys[0];
                    break;
                }
            }
        }
        else
        {
            key = inputNode.Keys[0];
        }
        return key;
    }
}