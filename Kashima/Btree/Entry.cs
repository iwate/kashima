using System;
using System.Collections.Generic;

namespace Kashima.BTree
{
    public partial class BTree<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Every entry in children contains either a key value pair or link to next child
        /// internal nodes: only use key and next
        /// external nodes: only use key and value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        [Serializable]
        private sealed class Entry<TKey, TValue> : IComparable
            where TKey : IComparable<TKey>
        {
            public TKey Key { get; set; }
            public IEnumerable<TValue> Values { get; set; }
            public Node<TKey, TValue> ChildNode { get; set; }
            public bool IsLeaf { get { return ChildNode == null;  } }

            public Entry(TKey key, TValue value, Node<TKey, TValue> next)
            {
                this.Key = key;
                this.Values = new TValue[] { value };
                this.ChildNode = next;
            }

            public int CompareTo(BTree<TKey, TValue>.Entry<TKey, TValue> other)
            {
                throw new NotImplementedException();
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
}
