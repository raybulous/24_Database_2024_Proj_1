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
        var newChild = Children[index].Insert(key, value); //pass the key to insert to the child node
        //gets a newChild if the insert lead to overflow and spliting was done
        if (newChild != null)
        {
            Children.Insert(index + 1, newChild); //Add the newChild to the Children list
            if (Children.Count == Keys.Count + 2 && !IsOverflow) //Add a key if required and won't lead to overflow
            {
                Keys.Insert(index, newChild.GetFirstKey());
            }
        }
        //split the the Child if overflow
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
        //newNode gets the second half of the keys and their corresponding children
        newNode.Keys.AddRange(Keys.GetRange(keyCount - degree / 2, degree / 2)); 
        newNode.Children.AddRange(Children.GetRange(childrenCount - childrenCount / 2, childrenCount / 2));
        //original removes key and children added to newNode
        Keys.RemoveRange(keyCount - temp, temp);
        Children.RemoveRange(childrenCount - childrenCount / 2, childrenCount / 2);
        //Include the newNode into the parent's key and children list
        TKey newKey = newNode.GetFirstKey();
        parent.Keys.Insert(index, newKey);
        parent.Children.Insert(index + 1, newNode);
        //Update keys 
        for (int i = 0; i < Keys.Count; i++)
        {
            newKey = Children[i+1].GetFirstKey();
            Keys[i] = newKey;
        }
        for (int i = 0; i < parent.Keys.Count; i++)
        {
            newKey = parent.Children[i+1].GetFirstKey();
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
        if (index == Children.Count)
        {
            index--;
        }
        return index;
    }

    public override bool Delete(TKey key)
    {
        int index = FindChildIndexForKey(key);
        bool isDeleted;
        do //try deleting from child node, if not found move to next child 
        {
            isDeleted = Children[index++].Delete(key); 
        } while (!isDeleted && index < Children.Count); //TODO: CHECK KEY, IF MORE THAN KEY, TERMINATE (REDUCE UNNECESSARY SEARCHING)
        if (!isDeleted)
        {
            return false; // Key not found
        }

        //Check if child is underflowed and handle it.
        HandleChildUnderflow(index - 1);

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

    //Merges the current node with the provided siblingNode.
    public override void Merge(Node<TKey, TValue> siblingNode, TKey parentKey)
    {
        if (siblingNode is InternalNode<TKey, TValue> siblingInternalNode)
        {
            //Add the parentKey that was separating the two nodes
            this.Keys.Add(parentKey);

            //Merge the keys from the sibling node
            this.Keys.AddRange(siblingInternalNode.Keys);

            //Merge the children from the sibling node
            this.Children.AddRange(siblingInternalNode.Children);
        }
    }

    protected void HandleChildUnderflow(int childIndex)
    {
        Node<TKey, TValue> child = Children[childIndex];
        Node<TKey, TValue> leftSibling = childIndex > 0 ? Children[childIndex - 1] : null;
        Node<TKey, TValue> rightSibling = childIndex < Children.Count - 1 ? Children[childIndex + 1] : null;

        //left merge with mid
        if (leftSibling != null && ((leftSibling is InternalNode<TKey, TValue> internalLeft && child is InternalNode<TKey, TValue> internalChild && internalLeft.Children.Count + internalChild.Children.Count <= BPlusTree<TKey, TValue>.degree + 1) || (leftSibling is LeafNode<TKey, TValue> leafLeft && child is LeafNode<TKey, TValue> leafChild && leafLeft.Values.Count + leafChild.Values.Count <= BPlusTree<TKey, TValue>.degree)))
        {
            leftSibling.Merge(child, this.Keys[childIndex - 1]);
            this.Children.RemoveAt(childIndex);
            this.Keys.RemoveAt(childIndex - 1);
        }
        //mid merge with right
        else if (rightSibling != null && ((rightSibling is InternalNode<TKey, TValue> internalRight && child is InternalNode<TKey, TValue> internalChild1 && internalRight.Children.Count + internalChild1.Children.Count <= BPlusTree<TKey, TValue>.degree + 1) || (rightSibling is LeafNode<TKey, TValue> leafRight && child is LeafNode<TKey, TValue> leafChild1 && leafRight.Values.Count + leafChild1.Values.Count <= BPlusTree<TKey, TValue>.degree)))
        {
            child.Merge(rightSibling, this.Keys[childIndex]);
            this.Children.RemoveAt(childIndex + 1);
            this.Keys.RemoveAt(childIndex);
        }
        else
        {
            //mid borrow key from left
            if (leftSibling != null && leftSibling.Keys.Count > MinKeys() && leftSibling.Keys.Count - child.Keys.Count > 1)
            {
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
            //mid borrow key from right
            else if (rightSibling != null && rightSibling.Keys.Count > MinKeys() && rightSibling.Keys.Count - child.Keys.Count > 1)
            {
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
    }

    protected void UpdateKeysAfterDeletion()
    {
        for (int i = 0; i < this.Children.Count; i++)
        {
            if (this.Children[i] is InternalNode<TKey, TValue> internalNode)
            {
                for (int j = 0; j < internalNode.Children.Count - 1; j++)
                {
                    internalNode.Keys[j] = internalNode.Children[j + 1].GetFirstKey();
                }
            }
        }
    }

    public override TKey GetFirstKey() //traverse down to Children[0] and LeafNode will return first key
    {
        var child = this.Children[0];
        return child.GetFirstKey();
    }

    protected override int MinKeys() //Calculate the minimum key given the b+ tree degree
    {
        return (int)Math.Floor(BPlusTree<TKey, TValue>.degree / 2.0);
    }
}