using ABB.SrcML.Data;
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
            //get test cases from projects
            detectTestCases();
            //get missing ones from target
            setMissingTestCases();
            //get summary of missing testcases i.e. tests under unit
            analyzeProjectTestCases(sourceProject, sourceTestCaseDetector.AllTestCases, sourceTestSummary);
            analyzeProjectTestCases(targetProject, targetTestCaseDetector.AllTestCases, targetTestSummary);

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

        public void analyzeProjectTestCases(string projectLocation,HashSet<TestCaseID> projectTestCases, HashSet<TestCaseSummary> projectTestSummary)
        {
            Console.WriteLine($"Analysing project {Path.GetFileName(projectLocation)}");

            using (var project = new DataProject<CompleteWorkingSet>(projectLocation, projectLocation, srcMLexeLocation))
            {
                project.Update();
                NamespaceDefinition globalNamespace;
                project.WorkingSet.TryObtainReadLock(5000, out globalNamespace);
                try
                {

                    // Step 1.   Build the call graph
                    Console.WriteLine("======  project summary ========= ");
                    Console.WriteLine("{0,10:N0} files", project.Data.GetFiles().Count());
                    Console.WriteLine("{0,10:N0} namespaces/packages", globalNamespace.GetDescendants<NamespaceDefinition>().Count());
                    Console.WriteLine("{0,10:N0} types", globalNamespace.GetDescendants<TypeDefinition>().Count());
                    Console.WriteLine("{0,10:N0} methods", globalNamespace.GetDescendants<MethodDefinition>().Count());
                    //Console.Read();
                    var methods = globalNamespace.GetDescendants<MethodDefinition>();
                    int i = 0;
                    // Step 2.   Testing
                    Console.WriteLine("======  analysing methods and identifying test-case methods ========= ");
                    foreach (MethodDefinition method in methods)
                    {
                        Console.WriteLine("Method Name : {0}", method.GetFullName());
                        //colect basic ID information
                        var declaringClass = method.GetAncestors<TypeDefinition>().FirstOrDefault();
                        var className = "";
                        if (declaringClass != null)
                        {
                            className = declaringClass.Name;
                        }
                        var nameSpaceName = GetNamespaceByMethod(method);
 
                        if (IsTestCase(method, projectTestCases))
                        {

                            SwumSummary swumSummary = new SwumSummary(method);
                            swumSummary.BasicSummary();
                            TestCaseAnalyzer analyzer = new TestCaseAnalyzer(method);
                            //analyzer.GetTestingObject();
                            string desc = swumSummary.Describe();
                            //writetext.WriteLine(method.Name + "  ,  " + desc);
                            //Console.WriteLine(nameSpaceName + "," + className + "," + method.Name + "Swum Description : " + desc);

                            TestCaseSummary tcSummary = new TestCaseSummary(desc, analyzer.ListAssertInfo, method);
                            tcSummary.NameSpaceName = nameSpaceName;
                            tcSummary.ClassName = className;
                            tcSummary.MethodName = method.Name;

                            projectTestSummary.Add(tcSummary);
                            var stmts = method.GetDescendants<Statement>();

                            //delete me
                            //using (StreamWriter sw = File.AppendText(@"D:\d.csv"))
                            //{
                            //   var number = stmts.Count() + 2;
                            //   sw.WriteLine(method.Name + "," + number);
                            //    sw.Close();
                            //}	
                            i++;
                        }


                    }


                }
                finally
                {
                    project.WorkingSet.ReleaseReadLock();
                }


            }


        }

        private bool IsTestCase(MethodDefinition md, HashSet<TestCaseID> projectTestCases)
        {
            var declaringClass = md.GetAncestors<TypeDefinition>().FirstOrDefault();
            string nameSpace = GetNamespaceByMethod(md);


            if (declaringClass == null || nameSpace == null)
            {
                return false;
            }
            foreach (var id in projectTestCases)
            {
                //Console.WriteLine("namespace id : " + id.NamespaceName + "namespace : " + nameSpace);
                if (id.MethodName == md.Name && id.ClassName == declaringClass.Name && id.NamespaceName == nameSpace)
                {
                    return true;
                }

            }
            return false;

        }


        private string GetNamespaceByMethod(MethodDefinition md)
        {
            var allNameSpace = md.GetAncestors<NamespaceDefinition>();
            string nameSpace = "";
            foreach (var ns in allNameSpace)
            {
                if (ns.Name == "")
                {
                    continue;
                }
                if (nameSpace == "")
                {
                    nameSpace += ns.Name;
                }
                else
                {
                    nameSpace = ns.Name + "." + nameSpace;
                }

            }
            return nameSpace;
        }
    }
}
