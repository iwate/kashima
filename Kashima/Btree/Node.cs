using System;

namespace Kashima.BTree
{
    [Serializable]
    public struct Node<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        public NodeType Type;
        public TKey Key;
        public TValue[] Values;
        public Node<TKey, TValue>[] Children;
        public int Count;
    }
}
