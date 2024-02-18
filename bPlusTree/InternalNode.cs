public class InternalNode<TKey, TValue> : Node<TKey, TValue> where TKey : IComparable<TKey>
{
    public List<Node<TKey, TValue>> Children { get; private set; }

    public InternalNode()
    {
        Keys = new List<TKey>();
        Children = new List<Node<TKey, TValue>>();
    }

    public override bool IsOverflow { get { return Keys.Count > BPlusTree<TKey, TValue>.degree; } }

    public override void Insert(TKey key, TValue value)
    {
        int index = FindIndexToInsert(key);
        if(index == Children.Count){
            index--;
        }   
        Children[index].Insert(key, value);
        if (Children[index].IsOverflow)
        {
            Children[index].Split(this, index);
        }
    }

    public override TValue Search(TKey key)
    {
        int index = Keys.BinarySearch(key);
        int childIndex = index >= 0 ? index + 1 : ~index;
        return Children[childIndex].Search(key);
    }

    public override void Split(InternalNode<TKey, TValue> parent, int index)
    {
        var newNode = new InternalNode<TKey, TValue>();
        int degree = BPlusTree<TKey, TValue>.degree;
        newNode.Keys.AddRange(Keys.GetRange(degree / 2, degree - degree / 2));
        newNode.Children.AddRange(Children.GetRange(degree / 2, degree - degree / 2));
        Keys.RemoveRange(degree / 2, degree - degree / 2);
        Children.RemoveRange(degree / 2, degree - degree / 2);
        parent.Keys.Insert(index, newNode.Keys[0]);
        parent.Children.Insert(index + 1, newNode);
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
}