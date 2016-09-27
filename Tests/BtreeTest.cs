using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using Kashima.BTree;
using Newtonsoft.Json;

namespace Tests
{
    [TestClass]
    public class BtreeTest
    {
        const int TEST_SIZE = 100000;
        IDictionary<long, Sample> Samples;
        Action<string> Log;
        [TestInitialize]
        public void Init()
        {
            Samples = Sample.CreateSamples(TEST_SIZE).ToDictionary(s => s.Id, s => s);
            Log = Console.WriteLine;
        }
        [TestMethod]
        public void Count_of_joined_is_equals_to_count_of_linqJoned()
        {
            var sw = new Stopwatch();
            sw.Start();
            var linq = Samples.Join(Samples, o => o.Key, i => i.Key, (o, i) => new { Outer = o, Inner = i }).ToArray();
            sw.Stop();
            Log($"linq joined count={linq.Length}, time={sw.ElapsedMilliseconds}[ms]");
            sw.Reset();
            
            var btree = new BPlusTree<long, long>(Samples.ToDictionary(d => d.Key, d => d.Key));

            sw.Start();
            var result = Join(Samples, Samples, o => o.Id, i => i.Id, (o, i) => new { Outer = o, Inner = i }, btree).ToArray();
            sw.Stop();
            Log($"joined count={result.Length}, time={sw.ElapsedMilliseconds}[ms]");
            sw.Reset();

            Assert.AreEqual(linq.Length, result.Length);
        }

        [TestMethod]
        public void Count_of_filtered_is_equals_to_count_of_linqWhered()
        {
            var start = new DateTime(2016, 1, 1);
            var end = new DateTime(2016, 1, 8);

            var sw = new Stopwatch();
            sw.Start();
            var linq = Samples.Where(s => start < s.Value.DateTime && s.Value.DateTime < end).ToArray();
            sw.Stop();
            Log($"linq whered count={linq.Length}, time={sw.ElapsedMilliseconds}[ms]");
            sw.Reset();
            
            var btree = new BPlusTree<DateTime, long>();
            foreach(var s in Samples)
            {
                btree.Add(s.Value.DateTime, s.Key);
            }

            sw.Start();
            var result = btree.GtAndLt(start, end).Select(id => Samples[id]).ToArray();
            sw.Stop();
            Log($"filtered count={result.Length}, time={sw.ElapsedMilliseconds}[ms]");
            sw.Reset();

            Assert.AreEqual(linq.Length, result.Length);
        }

        [TestMethod]
        public void Created_tree_from_node_is_valid()
        {
            var btree = new BPlusTree<DateTime, long>();
            foreach (var s in Samples)
                btree.Add(s.Value.DateTime, s.Key);

            var json = JsonConvert.SerializeObject(btree.RootNode);
            var node = JsonConvert.DeserializeObject<Node<DateTime, long>?>(json);
            var @new = new BPlusTree<DateTime, long>(node);
            Assert.AreNotEqual(0, @new.RootNode.Value.Count);
        }

        #region helpers
        public static IEnumerable<TResult> Join<TKey, TOuter, TInner, TIndex, TResult>(
            IDictionary<TKey, TOuter> outer,
            IDictionary<TKey, TInner> inner,
            Func<TOuter, TIndex> outKeySelector,
            Func<TInner, TIndex> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            BPlusTree<TIndex, TKey> btree)
            where TIndex : IComparable<TIndex>
        {
            var outerEnum = outer.GetEnumerator();
            outerEnum.Reset();
            while (outerEnum.MoveNext())
            {
                var item = outerEnum.Current.Value;
                var g = btree.Find(outKeySelector(item)).Select(id => inner[id]).ToArray();
                if (g != null)
                {
                    for (int i = 0; i < g.Length; i++)
                    {
                        yield return resultSelector(item, g[i]);
                    }
                }
            }
        }
        #endregion
    }
}
