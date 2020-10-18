using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.DataAnalysis
{
    public class TestCasePresence
    {
        private string ecosystem, project, testCaseFullName;
        private PresenceType presence;

        public TestCasePresence()
        {
        }

        public TestCasePresence(string ecosystem, string project, string testCaseFullName, PresenceType presence)
        {
            this.ecosystem = ecosystem;
            this.project = project;
            this.testCaseFullName = testCaseFullName;
            this.presence = presence;
        }

        public string Ecosystem { get => ecosystem; set => ecosystem = value; }
        public string Project { get => project; set => project = value; }
        public string TestCaseFullName { get => testCaseFullName; set => testCaseFullName = value; }
        public PresenceType Presence { get => presence; set => presence = value; }
    }
}
