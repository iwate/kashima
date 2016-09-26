using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kashima.BTree
{
    struct Node<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        public NodeType Type;
        public TKey Key;
        public TValue[] Values;
        public Node<TKey, TValue>[] Children;
        public int Count;
    }
}
