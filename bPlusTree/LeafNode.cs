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

        public override bool IsOverflow { get { return Keys.Count > BPlusTree<TKey, TValue>.degree - 1; } }

        public override void Insert(TKey key, TValue value)
        {
            int index = 0;
            while (index < Keys.Count && key.CompareTo(Keys[index]) >= 0)
            {
                index++;
            }
            Keys.Insert(index, key);
            Values.Insert(index, value);
        }

        public override TValue Search(TKey key)
        {
            int index = Keys.BinarySearch(key);
            if (index >= 0)
                return Values[index];
            else
                return default(TValue);
        }

        public override void Split(InternalNode<TKey, TValue> parent, int index)
        {
            var newNode = new LeafNode<TKey, TValue>();
            int degree = BPlusTree<TKey, TValue>.degree;
            newNode.Keys.AddRange(Keys.GetRange(degree / 2, degree - degree / 2));
            newNode.Values.AddRange(Values.GetRange(degree / 2, degree - degree / 2));
            Keys.RemoveRange(degree / 2, degree - degree / 2);
            Values.RemoveRange(degree / 2, degree - degree / 2);
            parent.Keys.Insert(index, newNode.Keys[0]);
            parent.Children.Insert(index + 1, newNode);
            newNode.Next = Next;
            Next = newNode;
        }
    }