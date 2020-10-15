using ABB.SrcML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.UnitTestScribe.Summary;
using WM.UnitTestScribe.TestCaseDetector;

namespace TeaCap.Propagation
{
   public class Utilities
    {
        public static HashSet<Statement> GetFocalMethodStatements(List<AssertSTInfo> listAssertInfo)
        {
            HashSet<Statement> focalMethodSet = new HashSet<Statement>();
            foreach (var assertInfo in listAssertInfo)
            {

                if (assertInfo.focalStmt != null)
                {                    

                    focalMethodSet.Add(assertInfo.focalStmt);
                }
            }
            return focalMethodSet;
        }

        public static Dictionary<Statement, List<Statement>> GetFocalToAssert(List<AssertSTInfo> listAssertInfo)
        {
            Dictionary<Statement, List<Statement>> focalToAssert = new Dictionary<Statement, List<Statement>>();
            foreach (var assertInfo in listAssertInfo)
            {

                if (assertInfo.focalStmt != null)
                {
                    var fMethod = assertInfo.focalStmt;
                    if (!focalToAssert.ContainsKey(fMethod))
                    {
                        focalToAssert[fMethod] = new List<Statement>();
                        focalToAssert[fMethod].Add(assertInfo.AssertStatment);
                    }
                    else
                    {
                        focalToAssert[fMethod].Add(assertInfo.AssertStatment);
                    }

                    
                }
            }
            return focalToAssert;
        }

        public static void printMethodDetails(MethodDefinition method, string className, string nameSpaceName, Statement statement)
        {
            var testMethod = statement.GetAncestorsAndSelf<MethodDefinition>().FirstOrDefault();
            var mdCalls = from statments in testMethod.GetDescendantsAndSelf()
                          from expression in statments.GetExpressions()
                          from call in expression.GetDescendantsAndSelf<MethodCall>()
                          select call;
            for (int j = mdCalls.Count() - 1; j >= 0; j--)
            {
                var call = mdCalls.ElementAt(j);
                var mdCall = call.FindMatches().FirstOrDefault() as MethodDefinition;
                if (mdCall == null)
                {
                    continue;
                }
                var decClass = mdCall.GetAncestors<TypeDefinition>().FirstOrDefault();
                var mdClass = "";
                if (decClass != null)
                {
                    mdClass = decClass.Name;
                }
                var mdPackage = GetNamespaceByMethod(mdCall);
                //Console.WriteLine($"mdCall {mdCall}");
              
                Console.WriteLine($"{nameSpaceName};{className};{method.Name};{method};{mdPackage};{mdClass};{mdCall.Name};{mdCall}");

            }
        }
        public static void printMethodDetails(StreamWriter testsUUTFile,string projectGitName, string parent, bool isFork, MethodDefinition method, string className, string nameSpaceName, Statement statement)
        {
            
            var testMethod = statement.GetAncestorsAndSelf<MethodDefinition>().FirstOrDefault();
            var mdCalls = from statments in testMethod.GetDescendantsAndSelf()
                          from expression in statments.GetExpressions()
                          from call in expression.GetDescendantsAndSelf<MethodCall>()
                          select call;
            for (int j = mdCalls.Count() - 1; j >= 0; j--)
            {
                var call = mdCalls.ElementAt(j);
                var mdCall = call.FindMatches().FirstOrDefault() as MethodDefinition;
                if (mdCall == null)
                {
                    continue;
                }
                
                var decClass = mdCall.GetAncestors<TypeDefinition>().FirstOrDefault();
                var mdClass = "";
                if (decClass != null)
                {
                    mdClass = decClass.Name;
                }
                var mdPackage = GetNamespaceByMethod(mdCall);
                testsUUTFile.WriteLine($"{projectGitName};{parent};{isFork};{nameSpaceName};{className};{method.Name};{method};{mdPackage};{mdClass};{mdCall.Name};{mdCall}");
                testsUUTFile.Flush();
            }
        }
        public static void printMethodDetails(StreamWriter testsUUTFile, string projectGitName, string parent, bool isFork, MethodDefinition method, string className, string nameSpaceName, MethodDefinition externalMethod)
        {
            

            var mdCall = externalMethod;
                
                
                var decClass = mdCall.GetAncestors<TypeDefinition>().FirstOrDefault();
                var mdClass = "";
                if (decClass != null)
                {
                    mdClass = decClass.Name;
                }
                var mdPackage = GetNamespaceByMethod(mdCall);
                testsUUTFile.WriteLine($"{projectGitName};{parent};{isFork};{nameSpaceName};{className};{method.Name};{method};{mdPackage};{mdClass};{mdCall.Name};{mdCall}");
                testsUUTFile.Flush();
            
        }
        public static bool IsTestCase(MethodDefinition md, HashSet<TestCaseID> projectTestCases)
        {
            var declaringClass = md.GetAncestors<TypeDefinition>().FirstOrDefault();            
            string nameSpace = GetNamespaceByMethod(md);  
            if (declaringClass == null || nameSpace == null)
            {
                return false;
            }
            TestCaseID found = projectTestCases.FirstOrDefault(t => t.ClassName == declaringClass.Name && t.MethodName == md.Name && string.IsNullOrWhiteSpace(t.NamespaceName)?true:t.NamespaceName==nameSpace);
            return found != null;
            //foreach (var id in projectTestCases)
            //{
            //    //Console.WriteLine("namespace id : " + id.NamespaceName + "namespace : " + nameSpace);
            //    if (id.MethodName == md.Name && id.ClassName == className && id.NamespaceName == nameSpace)
            //    {
            //        return true;
            //    }

            //}
            //return false;

        }


        public static string GetNamespaceByMethod(MethodDefinition md)
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

        private static void printMethod(StreamWriter classesWriter,string projectGitName, string parent, bool isFork, string package,string className, MethodDefinition method)
        {
            var parameters = from vars in method.Parameters
                     select vars.Name;
            classesWriter.WriteLine($"{projectGitName};{parent};{isFork};{package};{className};{method.Name};{parameters}");
            classesWriter.Flush();
        }
        public static void analyzeProjectTestCases(string projectLocation, HashSet<TestCaseID> projectTestCases, HashSet<TestCaseSummary> projectTestSummary, string srcMLexeLocation,string testsAndUUTFile,string classesAndMethodsFile,string projectGitName,string parent,bool isFork)
        {
            Console.WriteLine($"Analysing project {Path.GetFileName(projectLocation)}");

            using (var project = new DataProject<CompleteWorkingSet>(projectLocation, projectLocation, srcMLexeLocation))
            {
                project.Update();
                NamespaceDefinition globalNamespace;
                project.WorkingSet.TryObtainReadLock(5000, out globalNamespace);
                StreamWriter classesWriter = File.AppendText(classesAndMethodsFile);
                StreamWriter testsWriter = File.AppendText(testsAndUUTFile);
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
                        printMethod(classesWriter, projectGitName,parent,isFork, nameSpaceName, className, method);

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
                                    printMethodDetails(testsWriter, projectGitName, parent, isFork, method, className, nameSpaceName, statement);
                                }
                            }
                            else if (analyzer.ListAssertInfo != null && analyzer.ListAssertInfo.Count > 0)
                            {
                                //print all methods in asserts
                                foreach (AssertSTInfo info in analyzer.ListAssertInfo)
                                {
                                    printMethodDetails(testsWriter, projectGitName, parent, isFork, method, className, nameSpaceName, info.AssertStatment);
                                }
                            }
                            else
                            {
                                //all external methods referenced by the test method
                                HashSet<MethodDefinition> external = analyzer.InvokedExternalMethods;
                                foreach (MethodDefinition definition in external)
                                {
                                    if (definition == null)
                                    {
                                        continue;
                                    }
                                    printMethodDetails(testsWriter, projectGitName, parent, isFork, method, className, nameSpaceName, definition);
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


                }
                finally
                {
                    classesWriter.Close();
                    testsWriter.Close();
                    project.WorkingSet.ReleaseReadLock();
                }


            }


        }
    }
}
