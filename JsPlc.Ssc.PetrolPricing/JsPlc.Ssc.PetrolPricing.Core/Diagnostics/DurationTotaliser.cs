using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.Diagnostics
{
    public class DurationTotaliser
    {
        private class DurationTotal
        {
            private Stopwatch _stopwatch;

            public string Name { get; private set; }
            public long LastDurationMilliseconds { get; private set; }
            public long TotalDurationMilliseconds { get; set; }

            public DurationTotal(string name)
            {
                this.Name = name;
                this.LastDurationMilliseconds = 0;
                this.TotalDurationMilliseconds = 0;
            }

            public void Start()
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }

            public void Stop()
            {
                if (_stopwatch == null)
                    return;

                _stopwatch.Stop();
                this.LastDurationMilliseconds = _stopwatch.ElapsedMilliseconds;
                this.TotalDurationMilliseconds += this.LastDurationMilliseconds;
            }
        }

        private Dictionary<string, DurationTotal> _totals = new Dictionary<string, DurationTotal>();

        public void ResetAll()
        {
            _totals.Clear();
        }

        public void Start(string name)
        {
            var key = StandardiseKey(name);
            if (_totals.ContainsKey(key) == false)
                _totals[key] = new DurationTotal(key);
            _totals[key].Start();
        }

        public void Stop(string name)
        {
            var key = StandardiseKey(name);
            if (_totals.ContainsKey(key))
                _totals[key].Stop();
        }

        public Dictionary<string, long> GetTotals()
        {
            var totals = new Dictionary<string, long>();

            foreach (var kvp in _totals)
                totals.Add(kvp.Key, kvp.Value.TotalDurationMilliseconds);

            return totals;
        }

        public void WriteToFile(string filename)
        {
            EnsureDurationExists(filename);

            var totals = GetTotals();
            var sb = new StringBuilder();
            foreach (var kvp in totals)
                sb.AppendLine(kvp.Key + " = " + kvp.Value);

            System.IO.File.WriteAllText(filename, sb.ToString());
        }

        private void EnsureDurationExists(string filename)
        {
            var directory = System.IO.Path.GetDirectoryName(filename);
            if (System.IO.Directory.Exists(directory) == false)
                System.IO.Directory.CreateDirectory(directory);
        }

        private static string StandardiseKey(string name)
        {
            return name.ToLower();
        }
    }
}
