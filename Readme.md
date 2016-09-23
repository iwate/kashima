Kashima
===
!(master)[https://ci.appveyor.com/api/projects/status/ja84vdl13imnedui/branch/master?svg=true)

Kashima is Data structure library

sample:  
```cs
public static void Main(string[] args)
{
  var samples = CreateSamples(100000).ToDictionary(s => s.Id, s => s);
  var btree = new BTree<DateTime, long>();
  foreach(var s in Samples)
  {
      btree.Add(s.Value.DateTime, s.Key);
  }
  var start = new DateTime(2016, 1, 1);
  var end = new DateTime(2016, 1, 8);
  var result = btree.GtAndLt(start, end).Select(id => Samples[id]).ToArray();
}
class Sample
{
  public long Id { get; set; }
  public string Text { get; set; }
  public DateTime DateTime { get; set; }
}
static IEnumerable<Sample> CreateSamples(int size)
{
  /// ...
}
```


##License
MIT