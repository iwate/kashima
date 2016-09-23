using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Sample
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public DateTime DateTime { get; set; }

        public static IEnumerable<Sample> CreateSamples(int size)
        {
            var seed = 1024;
            var rand = new Random(seed);
            var anchorDate = new DateTime(2016, 1, 1).ToBinary();
            return Enumerable.Range(1, size).Select(i => new Sample
            {
                Id = i,
                Text = $"Sample Text {i}",
                DateTime = DateTime.FromBinary(anchorDate + rand.Next(-1000000, 1000000) * 3000000000)
            }).ToList();
        }
    }
}
