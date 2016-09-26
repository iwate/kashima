using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kashima.BTree
{
    public class BPlusTree<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        const int DEFAULT_SIZE = 10000;
        public int ChildrenSize { get; private set; }
        public int Height { get; private set; }
        NodeComparer<TKey, TValue> comparer = new NodeComparer<TKey, TValue>();
        Node<TKey, TValue> Root;
        int HalfSize;
        public BPlusTree() : this(DEFAULT_SIZE) { }
        public BPlusTree(int size)
        {
            Root.Type = NodeType.Node;
            Root.Children = new Node<TKey, TValue>[size];
            ChildrenSize = size;
            HalfSize = size / 2;
        }
        public BPlusTree(IEnumerable<KeyValuePair<TKey, TValue>> data, int size = DEFAULT_SIZE) : this(size)
        {
            foreach (var datum in data)
                Add(datum.Key, datum.Value);
        }

        Node<TKey, TValue> div(ref Node<TKey, TValue> node)
        {
            Array.Sort(node.Children, 0, node.Count, comparer);

            var tail = new Node<TKey, TValue> { Type = NodeType.Node, Children = new Node<TKey, TValue>[ChildrenSize] };
            for (int i = 0; i < HalfSize; i++)
                tail.Children[i] = node.Children[i + HalfSize];
            tail.Key = tail.Children[0].Key;

            var @new = new Node<TKey, TValue> { Type = NodeType.Node, Children = new Node<TKey, TValue>[ChildrenSize] };
            for(int i = 0; i < ChildrenSize; i++)
                @new.Type = NodeType.Node;

            node.Count = HalfSize;
            tail.Count = HalfSize;
            @new.Children[0] = node;
            @new.Children[1] = tail;
            @new.Count = 2;

            return @new;
        }
        Node<TKey, TValue>? addLeaf(ref Node<TKey, TValue> node, TKey key, TValue value)
        {
            int i = 0;
            for (; i < node.Count; i++)
            {
                var child = node.Children[i];
                var compared = child.Key.CompareTo(key);
                if (compared > 0)
                {
                    for (int j = node.Count; j > i; j--)
                        node.Children[j] = node.Children[j - 1];
                    node.Children[i] = new Node<TKey, TValue> { Key = key, Values = new TValue[] { value } };
                    node.Count++;
                    break;
                }
                else if (compared == 0)
                {
                    int len = child.Values.Length;
                    var values = new TValue[len + 1];
                    child.Values.CopyTo(values, 0);
                    values[len] = value;
                    node.Children[i].Values = values;
                    break;
                }
            }
            if (i == node.Count)
            {
                node.Children[i] = new Node<TKey, TValue> { Key = key, Values = new TValue[] { value } };
                node.Count++;
            }

            if( node.Count == ChildrenSize)
                return div(ref node);

            return null;
        }
        Node<TKey, TValue>? addNode(ref Node<TKey, TValue> node, TKey key, TValue value)
        {
            int index = node.Count - 1;
            for(int i = 1; i < node.Count; i++)
            {
                var compared = node.Children[i].Key.CompareTo(key);
                if (compared > 0)
                {
                    index = i - 1;
                    break;
                }
            }
            var @new = add(ref node.Children[index], key, value);
            if (@new.HasValue)
            {
                node.Children[node.Count++] = @new.Value.Children[1];
                Array.Sort(node.Children, 0, node.Count, comparer);
            }

            if (node.Count == ChildrenSize)
                return div(ref node);

            return null;
        }
        Node<TKey, TValue>? add(ref Node<TKey, TValue> node, TKey key, TValue value)
        {
            return node.Children[0].Type == NodeType.Leaf
                ? addLeaf(ref node, key, value)
                : addNode(ref node, key, value);
        }
        public void Add(TKey key, TValue value)
        {
            var @new = add(ref Root, key, value);
            if (@new.HasValue)
            {
                Root = @new.Value;
                Height++;
            }
        }



        public IEnumerable<TValue> Find(TKey key)
        {
            if (Root.Count == 0)
                return new TValue[0];

            return find(ref Root, key);
        }
        IEnumerable<TValue> find(ref Node<TKey, TValue> node, TKey key)
        {
            if (node.Children[0].Type == NodeType.Leaf)
            {
                for (var i = 0; i < node.Count; i++)
                {
                    var child = node.Children[i];
                    if (child.Key.CompareTo(key) == 0)
                        return child.Values;
                }
                return new TValue[0];
            }
            int index = node.Count - 1;
            for (var i = 1; i < node.Count; i++)
            {
                if (node.Children[i].Key.CompareTo(key) > 0)
                {
                    index = i - 1;
                    break;
                }
            }
            return find(ref node.Children[index], key);
        }

        IEnumerable<TValue> search(ref Node<TKey, TValue> node, Func<TKey, int> predicate)
        {
            var ret = new List<TValue>();
            if (node.Children[0].Type == NodeType.Leaf)
            {
                for (var i = node.Count - 1; i >= 0; i--)
                {
                    var child = node.Children[i];
                    var result = predicate(child.Key);
                    if (result == 0)
                        ret.AddRange(child.Values);
                    else if (result < 0)
                        break;
                }
            }
            else
            {
                for (var i = 0; i < node.Count; i++)
                {
                    var child = node.Children[i];
                    var result = predicate(child.Key);
                    if (result > 0)
                        break;
                    else
                        ret.AddRange(search(ref node.Children[i], predicate));
                }
            }
            return ret;
        }
        public IEnumerable<TValue> Lt(TKey lt)
        {
            if (Root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _lt = key.CompareTo(lt);
                var isLt = _lt < 0;
                return isLt ? 0 : 1;
            };

            return search(ref Root, predicate);
        }
        public IEnumerable<TValue> Le(TKey le)
        {
            if (Root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _le = key.CompareTo(le);
                var isLe = _le < 0 || _le == 0;
                return isLe ? 0 : 1;
            };

            return search(ref Root, predicate);
        }
        public IEnumerable<TValue> Gt(TKey gt)
        {
            if (Root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _gt = key.CompareTo(gt);
                var isGt = _gt < 0;
                return isGt ? 0 : -1;
            };

            return search(ref Root, predicate);
        }
        public IEnumerable<TValue> Ge(TKey ge)
        {
            if (Root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _ge = key.CompareTo(ge);
                var isGe = _ge < 0 || _ge == 0;
                return isGe ? 0 : -1;
            };

            return search(ref Root, predicate);
        }
        public IEnumerable<TValue> GtAndLt(TKey gt, TKey lt)
        {
            var c = gt.CompareTo(lt);
            if (c == 0 || c > 0)
                throw new ArgumentException("'gt' need to be less than and equal to 'lt'; gt <= lt");
            if (Root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var isGt = key.CompareTo(gt) > 0;
                var isLt = key.CompareTo(lt) < 0;
                if (isGt && isLt)
                    return 0;

                return isGt ? 1 : -1;
            };

            return search(ref Root, predicate);
        }
        public IEnumerable<TValue> GeAndLt(TKey ge, TKey lt)
        {
            var c = ge.CompareTo(lt);
            if (c == 0 || c > 0)
                throw new ArgumentException("'gt' need to be less than and equal to 'lt'; gt <= lt");
            if (Root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _ge = key.CompareTo(ge);
                var isGe = _ge > 0 || _ge == 0;
                var isLt = key.CompareTo(lt) < 0;
                if (isGe && isLt)
                    return 0;

                return isGe ? 1 : -1;
            };

            return search(ref Root, predicate);
        }
        public IEnumerable<TValue> GtAndLe(TKey gt, TKey le)
        {
            var c = gt.CompareTo(le);
            if (c == 0 || c > 0)
                throw new ArgumentException("'gt' need to be less than and equal to 'lt'; gt <= lt");
            if (Root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _le = key.CompareTo(le);
                var isGt = key.CompareTo(gt) > 0;
                var isLe = _le < 0 || _le == 0;
                if (isGt && isLe)
                    return 0;

                return isGt ? 1 : -1;
            };

            return search(ref Root, predicate);
        }
        public IEnumerable<TValue> GeAndLe(TKey ge, TKey le)
        {
            var c = ge.CompareTo(le);
            if (c == 0 || c > 0)
                throw new ArgumentException("'gt' need to be less than and equal to 'lt'; gt <= lt");
            if (Root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _ge = key.CompareTo(ge);
                var _le = key.CompareTo(le);
                var isGe = _ge > 0 || _ge == 0;
                var isLe = _le < 0 || _le == 0;
                if (isGe && isLe)
                    return 0;

                return isGe ? 1 : -1;
            };

            return search(ref Root, predicate);
        }
    }
}
