public class LeafNode<TKey, TValue> : Node<TKey, TValue> where TKey : IComparable<TKey>
{
    public List<TValue> Values { get; private set; }
    public LeafNode<TKey, TValue> Next { get; set; }
    public InternalNode<TKey, TValue> Parent { get; set; } // Parent reference

    public LeafNode()
    {
        Keys = new List<TKey>();
        Values = new List<TValue>();
        Next = null;
    }
    
    public override TKey GetFirstKey()
    {
        return Keys.FirstOrDefault(); // Assuming Keys is a List<TKey>
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
    
    public override bool Delete(TKey key)
    {
        int index = Keys.BinarySearch(key);
        if (index < 0)
        {
            return false; // Key not found.
        }
        // Remove key and its value.
        Keys.RemoveAt(index);
        Values.RemoveAt(index);

        // Check for underflow and handle it if necessary (borrow or merge).
        HandleUnderflow();

        return true;
    }
    
    // Returns the left sibling of the current node, if it exists.
    protected LeafNode<TKey, TValue> GetLeftSibling()
    {
        if (this.Parent == null) return null;
        int myIndex = this.Parent.Children.IndexOf(this);
        if (myIndex > 0)
        {
            return this.Parent.Children[myIndex - 1] as LeafNode<TKey, TValue>;
        }
        return null;
    }
    
    // Returns the right sibling of the current node, if it exists.
    protected LeafNode<TKey, TValue> GetRightSibling()
    {
        if (this.Parent == null) return null;
        int myIndex = this.Parent.Children.IndexOf(this);
        if (myIndex < this.Parent.Children.Count - 1)
        {
            return this.Parent.Children[myIndex + 1] as LeafNode<TKey, TValue>;
        }
        return null;
    }
    
    // Updates the key in the parent node that points to this node.
    protected void UpdateParentKey(TKey oldKey, TKey newKey)
    {
        int index = this.Parent.Keys.FindIndex(k => k.Equals(oldKey));
        if (index == -1)
        {
            // Old key not found, indicating a potential inconsistency in the tree
            throw new InvalidOperationException($"Attempted to update a non-existent key in a parent node. This indicates a potential inconsistency within the B+ tree structure. Old Key: {oldKey}, New Key: {newKey}");
        }

        // Existing logic for updating the key
    }

    
    // Removes the current node from its parent's children list.
    protected void RemoveFromParent()
    {
        if (this.Parent != null)
        {
            int index = this.Parent.Children.IndexOf(this);
            if (index != -1)
            {
                this.Parent.Children.RemoveAt(index);
                this.Parent.Keys.RemoveAt(index - 1); // Adjust parent keys accordingly. Be careful with index bounds.
            }
        }
    }

    // Merge method
    // Merges the current node with the provided siblingNode.
    // Assumes that siblingNode is to the right of this node and that parentKey is the key from the parent node that divides the two.
    public override void Merge(Node<TKey, TValue> siblingNode, TKey parentKey)
    {
        if (siblingNode is LeafNode<TKey, TValue> siblingLeafNode)
        {
            // Merge the keys from the sibling node
            this.Keys.AddRange(siblingLeafNode.Keys);

            // Merge the children from the sibling node
            this.Values.AddRange(siblingLeafNode.Values);

            this.Next = siblingLeafNode.Next;

            // Note: After merging, you'll likely need to update the parent node to remove the reference to siblingNode.
            // This might include removing the parentKey from the parent's keys and the siblingNode from the parent's children,
            // and potentially triggering further merges or redistributions if the parent now violates B+ tree properties.
        }
    }
    
    protected void HandleUnderflow()
    {
        if (this.Keys.Count >= MinKeys()) return; // No underflow

        LeafNode<TKey, TValue> leftSibling = this.GetLeftSibling();
        LeafNode<TKey, TValue> rightSibling = this.GetRightSibling();

        if (leftSibling != null && leftSibling.Keys.Count > MinKeys())
        {
            // Borrow from left
            TKey borrowedKey = leftSibling.Keys.Last();
            TValue borrowedValue = leftSibling.Values.Last();
            leftSibling.Keys.Remove(borrowedKey);
            leftSibling.Values.Remove(borrowedValue);
            this.Keys.Insert(0, borrowedKey);
            this.Values.Insert(0, borrowedValue);
            this.UpdateParentKey(borrowedKey, this.Keys[0]);
        }
        else if (rightSibling != null && rightSibling.Keys.Count > MinKeys())
        {
            // Borrow from right
            TKey borrowedKey = rightSibling.Keys.First();
            TValue borrowedValue = rightSibling.Values.First();
            rightSibling.Keys.RemoveAt(0);
            rightSibling.Values.RemoveAt(0);
            this.Keys.Add(borrowedKey);
            this.Values.Add(borrowedValue);
            rightSibling.UpdateParentKey(this.Keys.Last(), borrowedKey);
        }
        else if (leftSibling != null)
        {
            // Merge with left
            foreach (var key in this.Keys)
            {
                leftSibling.Keys.Add(key);
                leftSibling.Values.Add(this.Values[this.Keys.IndexOf(key)]);
            }
            leftSibling.Next = this.Next; // Assuming a linked list of leaves
            this.RemoveFromParent();
        }
        else if (rightSibling != null)
        {
            // Merge with right
            foreach (var key in rightSibling.Keys)
            {
                this.Keys.Add(key);
                this.Values.Add(rightSibling.Values[rightSibling.Keys.IndexOf(key)]);
            }
            this.Next = rightSibling.Next;
            rightSibling.RemoveFromParent();
        }
    }

    private TKey FindKeyValue(Node<TKey, TValue> inputNode)
    {
        // Initialize a variable to hold the key. Assuming default(TKey) is a valid default for your key type.
        TKey key = default(TKey);
    
        // Check if the inputNode is an InternalNode.
        if (inputNode is InternalNode<TKey, TValue> internalNode)
        {
            // We need to find the left-most leaf node starting from this internal node.
            Node<TKey, TValue> currentNode = internalNode;
            while (currentNode != null)
            {
                if (currentNode is LeafNode<TKey, TValue> leafNode)
                {
                    // If the current node is a LeafNode, we take its first key.
                    if (leafNode.Keys.Count > 0)
                    {
                        key = leafNode.Keys[0];
                        break;
                    }
                }
                else if (currentNode is InternalNode<TKey, TValue> currentInternalNode)
                {
                    // If the current node is an InternalNode, we go down to its first child.
                    if (currentInternalNode.Children.Count > 0)
                    {
                        currentNode = currentInternalNode.Children[0];
                    }
                    else
                    {
                        // This should not happen in a well-formed B+ tree, but it's good to handle the case.
                        break;
                    }
                }
                else
                {
                    // If for some reason the node type is unknown, break the loop.
                    break;
                }
            }
        }
        else if (inputNode is LeafNode<TKey, TValue> leafNode)
        {
            // If the input node is directly a LeafNode, we simply take its first key.
            if (leafNode.Keys.Count > 0)
            {
                key = leafNode.Keys[0];
            }
        }
    
        return key;
    }
    protected override int MinKeys()
    {
        return (int)Math.Ceiling(BPlusTree<TKey, TValue>.degree / 2.0) - 1;
    }
}