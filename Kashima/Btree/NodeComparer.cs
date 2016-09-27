using System;
using System.Collections.Generic;

namespace Kashima.BTree
{
    class NodeComparer<TKey, TValue> : IComparer<Node<TKey, TValue>>
        where TKey : IComparable<TKey>
    {
        public int Compare(Node<TKey, TValue> x, Node<TKey, TValue> y)
        {
            return x.Key.CompareTo(y.Key);
        }
    }
}
