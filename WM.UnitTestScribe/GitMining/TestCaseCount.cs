using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.GitMining
{
   public class TestCaseCount
    {
        private string ecosystem, project;
        private int classCount, testMethodCount, testClassCount, uutMethodCount, uutClassCount;

        public string Ecosystem { get => ecosystem; set => ecosystem = value; }
        public string Project { get => project; set => project = value; }
        public int ClassCount { get => classCount; set => classCount = value; }
        public int TestMethodCount { get => testMethodCount; set => testMethodCount = value; }
        public int TestClassCount { get => testClassCount; set => testClassCount = value; }
        public int UutMethodCount { get => uutMethodCount; set => uutMethodCount = value; }
        public int UutClassCount { get => uutClassCount; set => uutClassCount = value; }
    }
}
