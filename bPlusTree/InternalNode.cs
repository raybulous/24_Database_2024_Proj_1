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
            if (Children.Count == Keys.Count + 2 && !IsOverflow)
            {
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
        bool isDeleted;
        do
        {
            isDeleted = Children[index++].Delete(key);
        } while (!isDeleted && index < Children.Count);
        if (!isDeleted)
        {
            return false; // Key not found
        }

        /*
        bool isDeleted = Children[index].Delete(key);
        if (!isDeleted)
        {
            return false; // Key not found in any child.
        }*/

        // Check if child is underflowed and handle it.
        HandleChildUnderflow(index - 1);

        // Remove the key from the parent node if necessary.
        UpdateKeysAfterDeletion();

        return true;
    }

    protected int FindChildIndexForKey(TKey key)
    {
        for (int i = 0; i < this.Keys.Count; i++)
        {
            if (key.CompareTo(this.Keys[i]) <= 0)
            {
                return i - 1 < 0 ? 0 : i - 1; //return previous child in case of duplicate leaking to previous child
            }
        }
        return this.Keys.Count - 1;
    }

    // Merge method
    // Merges the current node with the provided siblingNode.
    // Assumes that siblingNode is to the right of this node and that parentKey is the key from the parent node that divides the two.
    public override void Merge(Node<TKey, TValue> siblingNode, TKey parentKey)
    {
        if (siblingNode is InternalNode<TKey, TValue> siblingInternalNode)
        {
            // Add the parentKey that was separating the two nodes
            this.Keys.Add(parentKey);

            // Merge the keys from the sibling node
            this.Keys.AddRange(siblingInternalNode.Keys);

            // Merge the children from the sibling node
            this.Children.AddRange(siblingInternalNode.Children);

            // Note: After merging, you'll likely need to update the parent node to remove the reference to siblingNode.
            // This might include removing the parentKey from the parent's keys and the siblingNode from the parent's children,
            // and potentially triggering further merges or redistributions if the parent now violates B+ tree properties.
        }
    }

    protected void HandleChildUnderflow(int childIndex)
    {
        Node<TKey, TValue> child = Children[childIndex];
        Node<TKey, TValue> leftSibling = childIndex > 0 ? Children[childIndex - 1] : null;
        Node<TKey, TValue> rightSibling = childIndex < Children.Count - 1 ? Children[childIndex + 1] : null;

        if (leftSibling != null && ((leftSibling is InternalNode<TKey, TValue> internalLeft && child is InternalNode<TKey, TValue> internalChild && internalLeft.Children.Count + internalChild.Children.Count <= BPlusTree<TKey, TValue>.degree + 1) || (leftSibling is LeafNode<TKey, TValue> leafLeft && child is LeafNode<TKey, TValue> leafChild && leafLeft.Values.Count + leafChild.Values.Count <= BPlusTree<TKey, TValue>.degree)))
        {
            //if (leftSibling is InternalNode<TKey, TValue> internalLeftSibling && child is InternalNode<TKey, TValue> internalChild)
            //{
            leftSibling.Merge(child, this.Keys[childIndex - 1]);
            this.Children.RemoveAt(childIndex);
            this.Keys.RemoveAt(childIndex - 1); // Update keys in this internal node
            //}
        }
        else if (rightSibling != null && ((rightSibling is InternalNode<TKey, TValue> internalRight && child is InternalNode<TKey, TValue> internalChild1 && internalRight.Children.Count + internalChild1.Children.Count <= BPlusTree<TKey, TValue>.degree + 1) || (rightSibling is LeafNode<TKey, TValue> leafRight && child is LeafNode<TKey, TValue> leafChild1 && leafRight.Values.Count + leafChild1.Values.Count <= BPlusTree<TKey, TValue>.degree)))
        {
            //if (rightSibling is InternalNode<TKey, TValue> internalRightSibling && child is InternalNode<TKey, TValue> internalChild)
            //{
            child.Merge(rightSibling, this.Keys[childIndex]);
            this.Children.RemoveAt(childIndex + 1);
            this.Keys.RemoveAt(childIndex); // Update keys in this internal node
            //}
        }
        else
        {
            if (leftSibling != null && leftSibling.Keys.Count > MinKeys() && leftSibling.Keys.Count - child.Keys.Count > 1)
            {
                // Borrow from the left sibling
                TKey borrowedKey = leftSibling.Keys.Last();
                if (leftSibling is InternalNode<TKey, TValue> leftInternal && child is InternalNode<TKey, TValue> childInternal)
                {
                    Node<TKey, TValue> childNode = leftInternal.Children.Last();
                    leftInternal.Children.RemoveAt(leftInternal.Children.Count - 1);
                    childInternal.Children.Insert(0, childNode);
                }
                else if (leftSibling is LeafNode<TKey, TValue> leftLeaf && child is LeafNode<TKey, TValue> childLeaf)
                {
                    TValue childValue = leftLeaf.Values.Last();
                    leftLeaf.Values.RemoveAt(leftLeaf.Values.Count - 1);
                    childLeaf.Values.Insert(0, childValue);
                }
                leftSibling.Keys.RemoveAt(leftSibling.Keys.Count - 1);
                child.Keys.Insert(0, borrowedKey);
            }
            else if (rightSibling != null && rightSibling.Keys.Count > MinKeys() && rightSibling.Keys.Count - child.Keys.Count > 1)
            {
                // Borrow from the right sibling
                TKey borrowedKey = rightSibling.Keys.First();
                if (rightSibling is InternalNode<TKey, TValue> rightInternal && child is InternalNode<TKey, TValue> childInternal)
                {
                    Node<TKey, TValue> childNode = rightInternal.Children.First();
                    rightInternal.Children.RemoveAt(0);
                    childInternal.Children.Add(childNode);
                }
                else if (rightSibling is LeafNode<TKey, TValue> rightLeaf && child is LeafNode<TKey, TValue> childLeaf)
                {
                    TValue childValue = rightLeaf.Values.First();
                    rightLeaf.Values.RemoveAt(0);
                    childLeaf.Values.Add(childValue);
                }
                rightSibling.Keys.RemoveAt(0);
                child.Keys.Add(borrowedKey);
            }
        }
        /*
        if (leftSibling != null && leftSibling.Keys.Count > MinKeys && leftSibling.Keys.Count - child.Keys.Count > 1)
        {
            // Borrow from the left sibling
            TKey borrowedKey = leftSibling.Keys.Last();
            leftSibling.Keys.RemoveAt(leftSibling.Keys.Count - 1);
            child.Keys.Insert(0, borrowedKey);
        }
        else if (rightSibling != null && rightSibling.Keys.Count > MinKeys && rightSibling.Keys.Count - child.Keys.Count > 1)
        {
            // Borrow from the right sibling
            TKey borrowedKey = rightSibling.Keys.First();
            rightSibling.Keys.RemoveAt(0);
            child.Keys.Add(borrowedKey);
        }
        else
        {
            // Merge child with a sibling (prefer left if possible)
            if (leftSibling != null && leftSibling is InternalNode<TKey, TValue> internalLeftSibling && leftSibling.Keys.Count + child.Keys.Count <= BPlusTree<TKey, TValue>.degree)
            {
                // Ensure child is also an InternalNode before merging
                if (child is InternalNode<TKey, TValue> internalChild)
                {
                    internalLeftSibling.Merge(internalChild, this.Keys[childIndex - 1]);
                    this.Children.RemoveAt(childIndex);
                    this.Keys.RemoveAt(childIndex - 1); // Update keys in this internal node
                }
            }
            else if (rightSibling != null && rightSibling is InternalNode<TKey, TValue> internalRightSibling && rightSibling.Keys.Count + child.Keys.Count <= BPlusTree<TKey, TValue>.degree)
            {
                // Ensure child is also an InternalNode before merging
                if (child is InternalNode<TKey, TValue> internalChild)
                {
                    internalChild.Merge(internalRightSibling, this.Keys[childIndex]);
                    this.Children.RemoveAt(childIndex + 1);
                    this.Keys.RemoveAt(childIndex); // Update keys in this internal node
                }
            }
        }*/
    }

    protected void UpdateKeysAfterDeletion()
    {
        for (int i = 0; i < this.Children.Count; i++)
        {
            if (this.Children[i] is InternalNode<TKey, TValue> internalNode)
            {
                for (int j = 0; j < internalNode.Children.Count - 1; j++)
                {
                    internalNode.Keys[j] = internalNode.Children[j+1].GetFirstKey();
                }
            }
        }
        /*
        for (int i = 0; i < this.Children.Count - 1; i++)
        {
            this.Keys[i] = this.Children[i + 1].GetFirstKey();
        }*/
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

    protected override int MinKeys()
    {
        return (int)Math.Floor(BPlusTree<TKey, TValue>.degree / 2.0);
    }
}