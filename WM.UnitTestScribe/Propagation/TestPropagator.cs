using ABB.SrcML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaCap.Propagation;
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
            sourceTestSummary = new HashSet<TestCaseSummary>();
            targetTestSummary = new HashSet<TestCaseSummary>();


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

                    ////print test cases
                    //StreamWriter wt = File.AppendText(@"C:\testpropagation\clones\testCases.csv");
                    //wt.WriteLine($"nameSpaceName;className;methodName");
                    //wt.Flush();
                    //foreach(TestCaseID id in projectTestCases)
                    //{
                    //    wt.WriteLine($"{id.NamespaceName};{id.ClassName};{id.MethodName}");
                    //    wt.Flush();
                    //}
                    //wt.Close();
                    ////print methods
                    //StreamWriter w = File.AppendText(@"C:\testpropagation\clones\budMethods.csv");
                    //w.WriteLine($"nameSpaceName;className;methodName");
                    //w.Flush();

                    int i = 0;
                    // Step 2.   Testing
                    Console.WriteLine("\n======  analysing methods and identifying UUTs ========= ");
                    foreach (MethodDefinition method in methods)
                    {
                        
                        //colect basic ID information
                        var declaringClass = method.GetAncestors<TypeDefinition>().FirstOrDefault();
                        var className = "";
                        if (declaringClass != null)
                        {
                            className = declaringClass.Name;
                        }
                        var nameSpaceName = Utilities.GetNamespaceByMethod(method);
                        //w.WriteLine($"{nameSpaceName};{className};{method.Name}");
                        //w.Flush();
                        //continue;

                        if (Utilities.IsTestCase(method, projectTestCases))
                        {
                            //Console.WriteLine("Method Name : {0}", method.GetFullName());
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
                            
                            Dictionary<Statement, List<Statement>> focalToAssert = Utilities.GetFocalToAssert(analyzer.ListAssertInfo);
                            if (focalToAssert != null && focalToAssert.Count > 0)
                            {
                                foreach (Statement statement in focalToAssert.Keys)
                                {
                                  Utilities.printMethodDetails(method, className, nameSpaceName, statement);
                                
                                }
                            }
                            else
                            {
                                //print all methods in asserts
                                foreach (AssertSTInfo info in analyzer.ListAssertInfo)
                                {
                                    Utilities.printMethodDetails(method, className, nameSpaceName, info.AssertStatment);
                                }
                            }
                            //Console.WriteLine(tcSummary.GetBodyDescriptions());
                            //var stmts = method.GetDescendants<Statement>();
                            //Console.WriteLine($"===Printing local methods tested by {method.GetFullName()}===");
                            //HashSet<MethodDefinition> local = analyzer.InvokedLocalMethods;
                            //foreach(MethodDefinition definition in local)
                            //{
                            //    if (definition == null)
                            //    {
                            //        continue;
                            //    }
                            //    Console.WriteLine(definition.GetFullName());
                            //}
                            //Console.WriteLine($"===Printing external methods tested by {method.GetFullName()}===");
                            //HashSet<MethodDefinition> external = analyzer.InvokedExternalMethods;
                            //foreach (MethodDefinition definition in external)
                            //{
                            //    if (definition == null)
                            //    {
                            //        continue;
                            //    }
                            //    Console.WriteLine(definition.GetFullName());
                            //}

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
                    //w.Close();

                }
                finally
                {
                    project.WorkingSet.ReleaseReadLock();
                }


            }


        }

        
    }
}
