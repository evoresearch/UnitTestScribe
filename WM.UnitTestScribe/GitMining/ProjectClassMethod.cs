using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.GitMining
{
   public class ProjectClassMethod
    {
        private string project, parent, package, className, methodName, methodParameters;
        private bool projectIsFork;

        public string Project { get => project; set => project = value; }
        public string Parent { get => parent; set => parent = value; }
        public string Package { get => package; set => package = value; }
        public string ClassName { get => className; set => className = value; }
        public string ClassFullName { get => string.Format($"{package}.{className}"); }
        public string MethodName { get => methodName; set => methodName = value; }
        public string MethodFullName { get => string.Format($"{package}.{className}.{methodName}"); }
        public string MethodParameters { get => methodParameters; set => methodParameters = value; }
        public bool ProjectIsFork { get => projectIsFork; set => projectIsFork = value; }
    }
}
