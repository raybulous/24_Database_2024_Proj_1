public abstract class Node<TKey, TValue> where TKey : IComparable<TKey>
{
    public List<TKey> Keys { get; protected set; }

    public abstract bool IsOverflow { get; }
    public abstract InternalNode<TKey, TValue> Insert(TKey key, TValue value);
    public abstract TValue Search(TKey key);
    public abstract InternalNode<TKey, TValue> Split(InternalNode<TKey, TValue> parent, int index);
    public abstract bool Delete(TKey key);
    protected int MinKeys => (int)Math.Ceiling(BPlusTree<TKey, TValue>.degree / 2.0) - 1;
    public abstract TKey GetFirstKey();

}