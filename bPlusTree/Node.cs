public abstract class Node<TKey, TValue> where TKey : IComparable<TKey>
{
    public List<TKey> Keys { get; protected set; }

    public abstract bool IsOverflow { get; }
    public abstract void Insert(TKey key, TValue value);
    public abstract TValue Search(TKey key);
    public abstract void Split(InternalNode<TKey, TValue> parent, int index);
}