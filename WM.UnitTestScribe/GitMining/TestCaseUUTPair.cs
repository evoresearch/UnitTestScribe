using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.GitMining
{
   public class TestCaseUUTPair
    {
        private string project, parent, testPackage, testClassName, testMethodName, testMethodParameters, uutPackage, uutClassName, uutMethodName, uutMethodParameters;
        private bool projectIsFork;

        public string Project { get => project; set => project = value; }
        public string Parent { get => parent; set => parent = value; }
        public string TestPackage { get => testPackage; set => testPackage = value; }
        public string TestClassName { get => testClassName; set => testClassName = value; }
        public string TestFullClassName { get => string.Format($"{testPackage}.{testClassName}"); }
        public string TestMethodName { get => testMethodName; set => testMethodName = value; }
        public string TestFullMethodName { get => string.Format($"{testPackage}.{testClassName}.{testMethodName}"); }
        public string TestMethodParameters { get => testMethodParameters; set => testMethodParameters = value; }
        public string UutPackage { get => uutPackage; set => uutPackage = value; }
        public string UutClassName { get => uutClassName; set => uutClassName = value; }
        public string UutFullClassName { get => string.Format($"{uutPackage}.{uutClassName}"); }
        public string UutMethodName { get => uutMethodName; set => uutMethodName = value; }
        public string UutFullMethodName { get => string.Format($"{uutPackage}.{uutClassName}.{uutMethodName}");}
        public string UutMethodParameters { get => uutMethodParameters; set => uutMethodParameters = value; }
        public bool ProjectIsFork { get => projectIsFork; set => projectIsFork = value; }
    }
}
