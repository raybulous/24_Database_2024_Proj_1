public abstract class Node<TKey, TValue> where TKey : IComparable<TKey>
{
    public List<TKey> Keys { get; protected set; }

    public abstract bool IsOverflow { get; }
    public abstract InternalNode<TKey, TValue> Insert(TKey key, TValue value);
    public abstract TValue Search(TKey key);
    public abstract InternalNode<TKey, TValue> Split(InternalNode<TKey, TValue> parent, int index);
}