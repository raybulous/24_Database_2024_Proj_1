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

    public override TKey GetFirstKey() //return first key for finding new key value in internal node
    {
        return Keys.FirstOrDefault();
    }

    public override bool IsOverflow { get { return Keys.Count > BPlusTree<TKey, TValue>.degree; } }

    public override InternalNode<TKey, TValue> Insert(TKey key, TValue value)
    {
        int index = 0;
        while (index < Keys.Count && key.CompareTo(Keys[index]) >= 0) //find index to insert to
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
        //newNode has the 2nd half of the Keys and Values
        newNode.Keys.AddRange(Keys.GetRange(count - degree / 2, degree / 2));
        newNode.Values.AddRange(Values.GetRange(count - degree / 2, degree / 2));
        //original Node has the 1st half of the Key and Values
        Keys.RemoveRange(count - degree / 2, degree / 2);
        Values.RemoveRange(count - degree / 2, degree / 2);
        //Update parent to include the newNode
        parent.Children.Insert(index + 1, newNode);
        //Update each node to point to the correct next leafNode
        newNode.Next = Next;
        Next = newNode;
        if (parent.Keys.Count != degree || index == 0 || index == degree - 1) //Add a Key if necessary
        {
            parent.Keys.Insert(index, newNode.Keys[0]);
        }
        else //If adding key is not necessary, parent is split into 2 nodes 
        {
            var parent2 = new InternalNode<TKey, TValue>(); //right sibling of original parent Node
            //parent2 gets the 2nd half of the keys and children
            parent2.Keys.AddRange(parent.Keys.GetRange(degree / 2, degree / 2));
            parent2.Children.AddRange(parent.Children.GetRange((degree + 2) / 2, (degree + 2) / 2));
            //parent gets the 1st half of the keys and children
            parent.Keys.RemoveRange(degree / 2, degree / 2);
            parent.Children.RemoveRange((degree + 2) / 2, (degree + 2) / 2);

            //Update keys
            for (int i = 0; i < parent.Keys.Count; i++)
            {
                var newKey = parent.Children[i + 1].GetFirstKey();
                parent.Keys[i] = newKey;
            }
            for (int i = 0; i < parent2.Keys.Count; i++)
            {
                var newKey = parent2.Children[i + 1].GetFirstKey();
                parent2.Keys[i] = newKey;
            }
            return parent2; //return parent to eventually add parent node to children list
        }
        return null;
    }

    public override bool Delete(TKey key)
    {
        int index = Keys.BinarySearch(key);
        if (index < 0)
        {
            return false; //Key not found.
        }
        //Remove key and its value.
        Keys.RemoveAt(index);
        Values.RemoveAt(index);

        return true;
    }

    //Merges the current node with the provided siblingNode.
    public override void Merge(Node<TKey, TValue> siblingNode, TKey parentKey)
    {
        if (siblingNode is LeafNode<TKey, TValue> siblingLeafNode)
        {
            //Merge the keys and values from the sibling node
            this.Keys.AddRange(siblingLeafNode.Keys);
            this.Values.AddRange(siblingLeafNode.Values);

            //Update Next to point to correct leafNode
            this.Next = siblingLeafNode.Next;
        }
    }

    protected override int MinKeys()
    {
        return (int)Math.Ceiling(BPlusTree<TKey, TValue>.degree / 2.0) - 1;
    }
}