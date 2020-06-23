using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.UnitTestScribe.Summary;
using WM.UnitTestScribe.TestCaseDetector;

namespace TeaCap.TestPropagator
{
    public class TestPropagator
    {
        private string sourceProject, targetProject, srcMLexeLocation;
        private TestCaseDetector sourceTestCaseDetector, targetTestCaseDetector;
        public HashSet<TestCaseSummary> sourceTestSummary, targetTestSummary;
        private HashSet<TestCaseID> missingTestCases;

        public TestPropagator(string srcProject, string trgtProject, string srcMLLocation)
        {
            sourceProject = srcProject;
            targetProject = trgtProject;
            srcMLexeLocation = srcMLLocation;
            missingTestCases = new HashSet<TestCaseID>();


        }

        public void propagate()
        {
            detectTestCases();
            setMissingTestCases();
        }
        public void detectTestCases()
        {
           
            //detect test cases in source 
            sourceTestCaseDetector = new TestCaseDetector(sourceProject, srcMLexeLocation,"source");
            sourceTestCaseDetector.AnalyzeTestCases();
            sourceTestSummary = new HashSet<TestCaseSummary>();
            printTestCasesInProject(sourceProject, "present", sourceTestCaseDetector.AllTestCases);

            //detect test cases in target 
            targetTestCaseDetector = new TestCaseDetector(targetProject, srcMLexeLocation,"target");
            targetTestCaseDetector.AnalyzeTestCases();
            targetTestSummary = new HashSet<TestCaseSummary>();
            printTestCasesInProject(targetProject, "present", targetTestCaseDetector.AllTestCases);

        }

        public void setMissingTestCases()
        {
            //if target has no test cases then add all to missing
            if (targetTestCaseDetector.AllTestCases != null && targetTestCaseDetector.AllTestCases.Count <= 0)
            {
                missingTestCases = sourceTestCaseDetector.AllTestCases;
            }
            else
            {
                foreach (TestCaseID sourceTestCase in sourceTestCaseDetector.AllTestCases)
                {
                    TestCaseID targetTestCase = targetTestCaseDetector.
                        AllTestCases.FirstOrDefault(t => t.ClassName == sourceTestCase.ClassName && t.MethodName == sourceTestCase.MethodName && t.NamespaceName == sourceTestCase.NamespaceName);
                    if (targetTestCase == null)
                    {
                        missingTestCases.Add(sourceTestCase);
                    }
                }
            }

            printTestCasesInProject(targetProject, "missing", missingTestCases);

        }

        private void printTestCasesInProject(string project,string type, HashSet<TestCaseID> testCases)
        {
            Console.WriteLine("===print {0} testcases in {1}===", type, Path.GetFileName(project));
            foreach (var testCaseId in testCases)
            {
                Console.WriteLine(testCaseId.NamespaceName + "  " + testCaseId.ClassName + "  " + testCaseId.MethodName);
            }
        }
    }
}
