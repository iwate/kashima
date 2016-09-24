using System;
using System.Collections.Generic;

namespace Kashima.BTree
{
    public partial class BPlusTree<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        const int DEFAULT_MAX_CHILDREN_PER_NODE = 1000;
        public int Height { get; private set; } = 1;
        int max;
        Node<TKey, TValue> root;

        public BPlusTree() : this(DEFAULT_MAX_CHILDREN_PER_NODE)
        {
        }
        public BPlusTree(int maxChildrenPerNode)
        {
            max = maxChildrenPerNode;
            root = new Node<TKey, TValue>(max);
        }
        public BPlusTree(IEnumerable<KeyValuePair<TKey, TValue>> data, int maxChildrenPerNode = DEFAULT_MAX_CHILDREN_PER_NODE)
            :this(maxChildrenPerNode)
        {
            foreach (var datum in data)
                Add(datum.Key, datum.Value);
        }
        public void Add(TKey key, TValue value)
        {
            var node = add(root, key, value);
            if (node != null)
            {
                root = node;
                Height += 1;
            }
        }
        Node<TKey, TValue> add(Node<TKey, TValue> node, TKey key, TValue value)
        {
            if (node.Children[0] == null || node.Children[0].IsLeaf)
            {
                if (node.AddLeafEntry(key, value))
                {
                    return node.Div();
                }
                return null;
            }

            Node<TKey, TValue> newNode = null;
            Entry<TKey, TValue> target = node.Children[node.count - 1];
            int i = 1;
            for (; i < node.count; i++)
            {
                var prev = node.Children[i - 1];
                var child = node.Children[i];
                if (child.Key.CompareTo(key) > 0)
                {
                    target = prev;
                    break;
                }
            }

            newNode = add(target.ChildNode, key, value);
            if (newNode != null)
            {
                node.Children[node.count] = newNode.Children[1];
                node.count++;
                Array.Sort(node.Children, 0, node.count);
            }
            if (node.count == max)
            {
                return node.Div();
            }
            
            return null;
        }

        public IEnumerable<TValue> Find(TKey key)
        {
            if(root.Children.Length == 0)
                return new TValue[0];

            return find(root, key);
        }
        IEnumerable<TValue> find(Node<TKey, TValue>node, TKey key)
        {
            if (node.Children[0].IsLeaf)
            {
                for (var i = 0; i < node.count; i++)
                {
                    var child = node.Children[i];
                    if (child.Key.CompareTo(key) == 0)
                        return child.Values;
                }
                return new TValue[0];
            }
            for (var i = 1; i < node.count; i++)
            {
                var prev = node.Children[i - 1];
                var child = node.Children[i];
                if (child.Key.CompareTo(key) > 0)
                    return find(prev.ChildNode, key);
            }
            var last = node.Children[node.count - 1];
            return find(last.ChildNode, key);
        }

        IEnumerable<TValue> search(Node<TKey, TValue> node, Func<TKey, int> predicate)
        {
            var ret = new List<TValue>();
            if (node.Children[0].IsLeaf)
            {
                for (var i = node.count - 1; i >= 0; i--)
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
                for (var i = 0; i < node.count; i++)
                {
                    var child = node.Children[i];
                    var result = predicate(child.Key);
                    if (result > 0)
                        break;
                    else
                        ret.AddRange(search(child.ChildNode, predicate));
                }
            }
            return ret;
        }
        public IEnumerable<TValue> Lt(TKey lt)
        {
            if (root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _lt = key.CompareTo(lt);
                var isLt = _lt < 0;
                return isLt ? 0 : 1;
            };

            return search(root, predicate);
        }
        public IEnumerable<TValue> Le(TKey le)
        {
            if (root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _le = key.CompareTo(le);
                var isLe = _le < 0 || _le == 0;
                return isLe ? 0 : 1;
            };

            return search(root, predicate);
        }
        public IEnumerable<TValue> Gt(TKey gt)
        {
            if (root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _gt = key.CompareTo(gt);
                var isGt = _gt < 0;
                return isGt ? 0 : -1;
            };

            return search(root, predicate);
        }
        public IEnumerable<TValue> Ge(TKey ge)
        {
            if (root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _ge = key.CompareTo(ge);
                var isGe = _ge < 0 || _ge == 0;
                return isGe ? 0 : -1;
            };

            return search(root, predicate);
        }
        public IEnumerable<TValue> GtAndLt(TKey gt, TKey lt)
        {
            var c = gt.CompareTo(lt);
            if (c == 0 || c > 0)
                throw new ArgumentException("'gt' need to be less than and equal to 'lt'; gt <= lt");
            if (root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
              {
                  var isGt = key.CompareTo(gt) > 0;
                  var isLt = key.CompareTo(lt) < 0;
                  if (isGt && isLt)
                      return 0;

                  return isGt ? 1 : -1;
              };

            return search(root, predicate);
        }
        public IEnumerable<TValue> GeAndLt(TKey ge, TKey lt)
        {
            var c = ge.CompareTo(lt);
            if (c == 0 || c > 0)
                throw new ArgumentException("'gt' need to be less than and equal to 'lt'; gt <= lt");
            if (root.Children.Length == 0)
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

            return search(root, predicate);
        }
        public IEnumerable<TValue> GtAndLe(TKey gt, TKey le)
        {
            var c = gt.CompareTo(le);
            if (c == 0 || c > 0)
                throw new ArgumentException("'gt' need to be less than and equal to 'lt'; gt <= lt");
            if (root.Children.Length == 0)
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

            return search(root, predicate);
        }
        public IEnumerable<TValue> GeAndLe(TKey ge, TKey le)
        {
            var c = ge.CompareTo(le);
            if (c == 0 || c > 0)
                throw new ArgumentException("'gt' need to be less than and equal to 'lt'; gt <= lt");
            if (root.Children.Length == 0)
                return new TValue[0];

            Func<TKey, int> predicate = key =>
            {
                var _ge = key.CompareTo(ge);
                var _le = key.CompareTo(le);
                var isGe =  _ge > 0 || _ge == 0;
                var isLe = _le < 0 || _le == 0;
                if (isGe && isLe)
                    return 0;

                return isGe ? 1 : -1;
            };

            return search(root, predicate);
        }
    }
}
