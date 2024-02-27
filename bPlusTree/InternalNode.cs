public class InternalNode<TKey, TValue> : Node<TKey, TValue> where TKey : IComparable<TKey>
{
    public List<Node<TKey, TValue>> Children { get; private set; }
    private int MinKeys => (int)Math.Ceiling(BPlusTree<TKey, TValue>.degree / 2.0) - 1;
    public InternalNode<TKey, TValue> Parent { get; set; } // Parent reference



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

    
    public override bool Delete(TKey key)
    {
        int index = FindChildIndexForKey(key);
        bool isDeleted = Children[index].Delete(key);
        if (!isDeleted)
        {
            return false; // Key not found in any child.
        }

        // Check if child is underflowed and handle it.
        HandleChildUnderflow(index);

        // Remove the key from the parent node if necessary.
        UpdateKeysAfterDeletion();

        return true;
    }
    
    protected int FindChildIndexForKey(TKey key)
    {
        for (int i = 0; i < this.Keys.Count; i++)
        {
            if (key.CompareTo(this.Keys[i]) < 0)
            {
                return i;
            }
        }
        return this.Keys.Count; // The key is greater than all existing keys.
    }
    
    // Merge method
    // Merges the current node with the provided siblingNode.
    // Assumes that siblingNode is to the right of this node and that parentKey is the key from the parent node that divides the two.
    public void Merge(InternalNode<TKey, TValue> siblingNode, TKey parentKey)
    {
        // Add the parentKey that was separating the two nodes
        this.Keys.Add(parentKey);
        
        // Merge the keys from the sibling node
        this.Keys.AddRange(siblingNode.Keys);
        
        // Merge the children from the sibling node
        this.Children.AddRange(siblingNode.Children);

        // Update parent references for the children being merged, if necessary
        foreach (var child in siblingNode.Children)
        {
            if (child is InternalNode<TKey, TValue> internalChild)
            {
                internalChild.Parent = this; // Assuming each node has a Parent property
            }
            else if (child is LeafNode<TKey, TValue> leafChild)
            {
                leafChild.Parent = this; // Similarly, for leaf nodes
            }
        }

        // Note: After merging, you'll likely need to update the parent node to remove the reference to siblingNode.
        // This might include removing the parentKey from the parent's keys and the siblingNode from the parent's children,
        // and potentially triggering further merges or redistributions if the parent now violates B+ tree properties.
    }
    
    protected void HandleChildUnderflow(int childIndex)
    {
        Console.WriteLine($"Before HandleChildUnderflow - childIndex: {childIndex}");
        Console.WriteLine($"Children count: {this.Children.Count}");
        Console.WriteLine($"Entering HandleChildUnderflow - childIndex: {childIndex}");

        Node<TKey, TValue> child = Children[childIndex];
        Node<TKey, TValue> leftSibling = childIndex > 0 ? Children[childIndex - 1] : null;
        Node<TKey, TValue> rightSibling = childIndex < Children.Count - 1 ? Children[childIndex + 1] : null;
        
        if (this.Children[childIndex] == null) {
            Console.WriteLine($"Child at index {childIndex} is null");
        }
        if (childIndex > 0 && this.Children[childIndex - 1] == null) {
            Console.WriteLine($"Left sibling at index {childIndex - 1} is null");
        }
        if (childIndex < this.Children.Count - 1 && this.Children[childIndex + 1] == null) {
            Console.WriteLine($"Right sibling at index {childIndex + 1} is null");
        }
        
        // Ensure Children is not null
        if (Children == null)
        {
            Console.WriteLine("Children list is null.");
            return;
        }

        // Ensure childIndex is within the valid range
        if (childIndex < 0 || childIndex >= Children.Count)
        {
            Console.WriteLine($"Child index {childIndex} is out of range.");
            return;
        }

        // Check if the child and siblings are null
        if (Children[childIndex] == null)
        {
            Console.WriteLine($"Child at index {childIndex} is null");
        }
        if (childIndex > 0 && Children[childIndex - 1] == null)
        {
            Console.WriteLine($"Left sibling at index {childIndex - 1} is null");
        }
        if (childIndex < Children.Count - 1 && Children[childIndex + 1] == null)
        {
            Console.WriteLine($"Right sibling at index {childIndex + 1} is null");
        }


        if (leftSibling != null && leftSibling.Keys.Count > MinKeys)
        {
            Console.WriteLine($"Child key count before modification: {this.Children[childIndex].Keys.Count}");

            // Borrow from the left sibling
            TKey borrowedKey = leftSibling.Keys.Last();
            leftSibling.Keys.RemoveAt(leftSibling.Keys.Count - 1);
            child.Keys.Insert(0, borrowedKey);
            // Handle value or child pointer borrowing as necessary
            Console.WriteLine($"After modification - Children count: {this.Children.Count}");

        }
        else if (rightSibling != null && rightSibling.Keys.Count > MinKeys)
        {
            Console.WriteLine($"Child key count before modification: {this.Children[childIndex].Keys.Count}");
            // Borrow from the right sibling
            TKey borrowedKey = rightSibling.Keys.First();
            rightSibling.Keys.RemoveAt(0);
            child.Keys.Add(borrowedKey);
            // Handle value or child pointer borrowing as necessary
            Console.WriteLine($"After modification - Children count: {this.Children.Count}");

        }
        else
        {
            // Merge child with a sibling (prefer left if possible)
            if (leftSibling != null && leftSibling is InternalNode<TKey, TValue> internalLeftSibling)
            {
                // Ensure child is also an InternalNode before merging
                if (child is InternalNode<TKey, TValue> internalChild)
                {
                    internalLeftSibling.Merge(internalChild, this.Keys[childIndex - 1]);
                    this.Children.RemoveAt(childIndex);
                    this.Keys.RemoveAt(childIndex - 1); // Update keys in this internal node
                }
            }
            else if (rightSibling != null && rightSibling is InternalNode<TKey, TValue> internalRightSibling)
            {
                // Ensure child is also an InternalNode before merging
                if (child is InternalNode<TKey, TValue> internalChild)
                {
                    internalChild.Merge(internalRightSibling, this.Keys[childIndex]);
                    this.Children.RemoveAt(childIndex + 1);
                    this.Keys.RemoveAt(childIndex); // Update keys in this internal node
                }
            }
        }
    }
    
    protected void UpdateKeysAfterDeletion()
    {
        for (int i = 0; i < this.Children.Count - 1; i++)
        {
            this.Keys[i] = this.Children[i + 1].GetFirstKey();
        }
    }
    
    public override TKey GetFirstKey()
    {
        if (this.Children == null || this.Children.Count == 0)
        {
            throw new InvalidOperationException("Cannot get first key from an empty internal node.");
        }
        var child = this.Children[0];
        return child.GetFirstKey();
    }
}