using System;
using System.Linq;

namespace Kashima.BTree
{
    public partial class BTree<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        [Serializable]
        private sealed class Node<TKey, TValue>
            where TKey : IComparable<TKey>
        {
            internal int numberOfChildren;
            internal Entry<TKey, TValue>[] Children { get; set; }
            internal int count;

            public Node(int maximumNumberOfChildren)
            {
                count = 0;
                this.numberOfChildren = maximumNumberOfChildren;
                Children = new Entry<TKey, TValue>[maximumNumberOfChildren];
            }
            public Node<TKey, TValue> Div()
            {
                Array.Sort(Children, 0, count);
                var half = numberOfChildren / 2;
                var head = Children.Take(half).ToArray();
                var tail = Children.Skip(half).Take(numberOfChildren - half).ToArray();

                var headNode = this;
                for(var i = half; i < numberOfChildren; i++ ){
                    headNode.Children[i] = null;
                }
                head.CopyTo(headNode.Children, 0);
                headNode.count = head.Length;
                var tailNode = new Node<TKey, TValue>(numberOfChildren);
                tail.CopyTo(tailNode.Children, 0);
                tailNode.count = tail.Length;
                
                var headEntry = new Entry<TKey, TValue>(head[0].Key, default(TValue), headNode);
                var tailEntry = new Entry<TKey, TValue>(tail[0].Key, default(TValue), tailNode);

                var node = new Node<TKey, TValue>(numberOfChildren);
                node.Children[0] = headEntry;
                node.Children[1] = tailEntry;
                node.count = 2;
                return node;
            }
            public void ShiftToTail(int index)
            {
                for(var i = count; i > index; i--)
                {
                    Children[i] = Children[i - 1];
                }
            }
            public bool AddLeafEntry(TKey key, TValue value)
            {
                int i;
                for (i = 0; i < numberOfChildren; i++)
                {
                    var child = Children[i];
                    if(child == null)
                    {
                        Children[i] = new Entry<TKey, TValue>(key, value, null);
                        count = i + 1;
                        break;
                    }
                    var compared = child.Key.CompareTo(key);
                    if (compared == 0)
                    {
                        child.Values = child.Values.Concat(new TValue[] { value }).ToArray();
                        break;
                    }
                    if (compared > 0)
                    {
                        ShiftToTail(i);
                        Children[i] = new Entry<TKey, TValue>(key, value, null);
                        count++;
                        break;
                    }
                }
                //Array.Sort(Children, 0, count);
                return count >= numberOfChildren;
            }
        }
    }
}
