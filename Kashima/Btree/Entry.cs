using System;
using System.Collections.Generic;

namespace Kashima.BTree
{
    public class Entry<TKey, TValue> : IComparable
            where TKey : IComparable<TKey>
    {
        public TKey Key { get; set; }
        public IEnumerable<TValue> Values { get; set; }
        public Node<TKey, TValue> ChildNode { get; set; }
        public bool IsLeaf { get { return ChildNode == null; } }

        public Entry(TKey key, TValue value, Node<TKey, TValue> next)
        {
            this.Key = key;
            this.Values = new TValue[] { value };
            this.ChildNode = next;
        }

        public int CompareTo(object obj)
        {
            var entry = obj as Entry<TKey, TValue>;
            if (entry == null)
                return 1;
            else
                return Key.CompareTo(entry.Key);
        }
    }
}
