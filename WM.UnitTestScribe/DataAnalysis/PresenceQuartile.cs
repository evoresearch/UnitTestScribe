using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.DataAnalysis
{
    /// <summary>
    /// THis is used to keep track of test cases present or missing in 25%, 50%, 75%, and 100% of projects in an ecosystem
    /// </summary>
   public class PresenceQuartile
    {

        private string ecosystem;
        private List<int> p25, p50, p75, p100, m25, m50, m75, m100;

        public PresenceQuartile(string ecosystem)
        {
            this.ecosystem = ecosystem;
            p25 = new List<int>();p50 = new List<int>();p75 = new List<int>();p100 = new List<int>();
            m25 = new List<int>(); m50 = new List<int>(); m75 = new List<int>(); m100 = new List<int>();
        }

        public string Ecosystem { get => ecosystem; set => ecosystem = value; }
        public List<int> P25 { get => p25; set => p25 = value; }
        public List<int> P50 { get => p50; set => p50 = value; }
        public List<int> P75 { get => p75; set => p75 = value; }
        public List<int> P100 { get => p100; set => p100 = value; }
        public List<int> M25 { get => m25; set => m25 = value; }
        public List<int> M50 { get => m50; set => m50 = value; }
        public List<int> M75 { get => m75; set => m75 = value; }
        public List<int> M100 { get => m100; set => m100 = value; }
    }
}
