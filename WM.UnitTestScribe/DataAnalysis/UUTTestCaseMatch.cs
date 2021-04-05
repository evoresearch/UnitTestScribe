using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.DataAnalysis
{
   public class UUTTestCaseMatch
    {
        
        private string ecosystem;
        List<int> byClassName, byMethodName;

        public UUTTestCaseMatch(string ecosystem)
        {
            this.ecosystem = ecosystem;
            byClassName = new List<int>();
            byMethodName = new List<int>();
        }

        public string Ecosystem { get => ecosystem; set => ecosystem = value; }
        public List<int> ByClassName { get => byClassName; set => byClassName = value; }
        public List<int> ByMethodName { get => byMethodName; set => byMethodName = value; }
    }
}
