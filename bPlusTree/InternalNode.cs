public class InternalNode<TKey, TValue> : Node<TKey, TValue> where TKey : IComparable<TKey>
{
    public List<Node<TKey, TValue>> Children { get; private set; }

    public InternalNode()
    {
        Keys = new List<TKey>();
        Children = new List<Node<TKey, TValue>>();
    }

    public override bool IsOverflow { get { return Children.Count > BPlusTree<TKey, TValue>.degree + 1; } }

    public override InternalNode<TKey, TValue> Insert(TKey key, TValue value)
    {
        int index = FindIndexToInsert(key);
        if (index == Children.Count)
        {
            index--;
        }
        var newChild = Children[index].Insert(key, value);
        if (newChild != null)
        {
            Children.Insert(index + 1, newChild);
            if(Children.Count == Keys.Count + 2 && !IsOverflow){
                Keys.Insert(index, FindKeyValue(newChild));
            }
        }
        if (Children[index].IsOverflow)
        {
            var sibling = Children[index].Split(this, index);
            if (sibling != null)
            {
                return sibling;
            }
        }
        return null;
    }

    public override TValue Search(TKey key)
    {
        int index = Keys.BinarySearch(key);
        int childIndex = index >= 0 ? index + 1 : ~index;
        return Children[childIndex].Search(key);
    }

    public override InternalNode<TKey, TValue> Split(InternalNode<TKey, TValue> parent, int index)
    {
        var newNode = new InternalNode<TKey, TValue>();
        int degree = BPlusTree<TKey, TValue>.degree;
        int keyCount = Keys.Count;
        int childrenCount = Children.Count;
        int temp = (int)Math.Ceiling((double)keyCount / 2);
        newNode.Keys.AddRange(Keys.GetRange(keyCount - degree / 2, degree / 2));
        newNode.Children.AddRange(Children.GetRange(childrenCount - childrenCount / 2, childrenCount / 2));
        Keys.RemoveRange(keyCount - temp, temp);
        Children.RemoveRange(childrenCount - childrenCount / 2, childrenCount / 2);
        TKey newKey = FindKeyValue(newNode);
        parent.Keys.Insert(index, newKey);
        parent.Children.Insert(index + 1, newNode);
        for (int i = 0; i < Keys.Count; i++)
        {
            newKey = FindKeyValue(Children[i + 1]);
            Keys[i] = newKey;
        }
        for (int i = 0; i < parent.Keys.Count; i++)
        {
            newKey = FindKeyValue(parent.Children[i + 1]);
            parent.Keys[i] = newKey;
        }
        return null;
    }

    private int FindIndexToInsert(TKey key)
    {
        int index = 0;
        while (index < Keys.Count && key.CompareTo(Keys[index]) >= 0)
        {
            index++;
        }
        return index;
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