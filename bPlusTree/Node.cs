//Abstract class for B+ tree Nodes. Internal Node and Leaf Node 
public abstract class Node<TKey, TValue> where TKey : IComparable<TKey>
{
    //Internal Node and Leaf Node both have List of Keys for traversing
    public List<TKey> Keys { get; protected set; }
    //No second list of address - Internal Node has Children for going down the tree - Leaf Node has Value for pointing to disk
    public abstract bool IsOverflow { get; } //Check if node overflow and split is required
    public abstract InternalNode<TKey, TValue> Insert(TKey key, TValue value);
    public abstract TValue Search(TKey key);
    public abstract InternalNode<TKey, TValue> Split(InternalNode<TKey, TValue> parent, int index);
    public abstract bool Delete(TKey key);
    protected abstract int MinKeys(); //Return min num of keys for the type of node
    public abstract TKey GetFirstKey(); //Traverse down the tree on the left and return first key
    public abstract void Merge(Node<TKey, TValue> child, TKey key); 
}