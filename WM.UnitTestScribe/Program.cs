using System.Configuration;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.Swum;
using ABB.Swum.Nodes;
using Antlr3.ST;
using CommandLine.Text;
using WM.UnitTestScribe.CallGraph;
using WM.UnitTestScribe.Summary;
using WM.UnitTestScribe.TestCaseDetector;
using ABB.SrcML.Data;
using TeaCap.TestPropagator;
using TeaCap.GitMining;
using System.Threading;
using System.Globalization;

namespace WM.UnitTestScribe {
    public class Program {
        //Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        
        /// <summary> Subject application location </summary>
        //public static readonly string LocalProj = @"D:\Research\myTestSubjects\Callgraph\CallgraphSubject";
        //public static readonly string LocalProj = @"D:\Research\Subjects\google-api-dotnet-client-master";
        //public static readonly string LocalProj = @"D:\Research\Subjects\Sando-master";
        public static readonly string LocalProj = ConfigurationManager.AppSettings["sourceProjectFolder"];
        public static readonly string targetProject = ConfigurationManager.AppSettings["targetProjectFolder"];
        
        //public static readonly string LocalProj = @"D:\Research\Subjects\Glimpse-master";
        /// <summary> SrcML directory location </summary>
        public static readonly string SrcmlLoc = ConfigurationManager.AppSettings["srcMLexeFolder"];
        public static readonly string outputLoc = ConfigurationManager.AppSettings["srcMLCallGraphOutputFolder"];


        /// <summary>
        /// Command line testing
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args) {
            DateTime dt = DateTime.Now;
           //args = new string[] { "hello" };
           //args = new string[] { "testcases", "--loc", LocalProj, "--srcmlPath", SrcmlLoc }; 
           args = new string[] { "summary", "--loc", LocalProj, "--srcmlPath", SrcmlLoc, "--outputLoc",  outputLoc }; 
            var options = new Options();
            string invokedVerb = null;
            object invokedVerbOptions = null;


            if (!CommandLine.Parser.Default.ParseArguments(args, options,
                (verb, verbOptions) => {
                    invokedVerb = verb;
                    invokedVerbOptions = verbOptions;
                })) {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }
            if (invokedVerb == "callgraph") {
                var callGraphOp = (CallgraphOptions)invokedVerbOptions;
                var generator = new InvokeCallGraphGenerator(callGraphOp.LocationsPath, callGraphOp.SrcMLPath);
                generator.run();
            } else if (invokedVerb == "testcases") {
                var testCaseOp = (TestCaseDetectOptions)invokedVerbOptions;
                var detector = new TestCaseDetector.TestCaseDetector(testCaseOp.LocationsPath, testCaseOp.SrcMLPath);
                detector.AnalyzeTestCases();
                Console.WriteLine("print testcases");
                foreach (var testCaseId in detector.AllTestCases) {
                    Console.WriteLine(testCaseId.NamespaceName + "  "+ testCaseId.ClassName + "  " + testCaseId.MethodName);
                }
            } else if (invokedVerb == "summary") {
                var SummaryOp = (SummarizeTestOptions)invokedVerbOptions;
                //ProjectCloner projectCloner = new ProjectCloner(SrcmlLoc);
                //projectCloner.cloneRepos();

                DataAnalyzer dataAnalyzer = new DataAnalyzer();
                dataAnalyzer.CountEcoSystems();
                //TestPropagator testPropagator = new TestPropagator(LocalProj, targetProject, SrcmlLoc);
                //testPropagator.propagate();
                //var summary = new SummaryGenerator(SummaryOp.LocationsPath, SummaryOp.SrcMLPath);
                //Console.WriteLine("This is summary");
                //summary.AnalyzeSummary();
                //summary.GenerateSummary(SummaryOp.OutputLoc);
                //Console.WriteLine("Done!!!!!!  Thanks.");

            } else if (invokedVerb == "hello") {

                Console.WriteLine("Hello");
                
                //string proPath = @"C:\Users\boyang.li@us.abb.com\Documents\RunningTest\Input\ConsoleApplication1";
                //string proPath = @"C:\Users\boyang.li@us.abb.com\Documents\RunningTest\Input\SrcML\ABB.SrcML";
                using (var project = new DataProject<CompleteWorkingSet>(LocalProj, LocalProj, SrcmlLoc))
                {
                    project.Update();
                    NamespaceDefinition globalNamespace;
                    project.WorkingSet.TryObtainReadLock(5000, out globalNamespace);
                    try
                    {

                        // Step 1.   Build the call graph
                        Console.WriteLine("{0,10:N0} files", project.Data.GetFiles().Count());
                        Console.WriteLine("{0,10:N0} namespaces", globalNamespace.GetDescendants<NamespaceDefinition>().Count());
                        Console.WriteLine("{0,10:N0} types", globalNamespace.GetDescendants<TypeDefinition>().Count());
                        Console.WriteLine("{0,10:N0} methods", globalNamespace.GetDescendants<MethodDefinition>().Count());
                        Console.ReadLine();
                        var methods = globalNamespace.GetDescendants<MethodDefinition>();

                        // Step 2.   Testing
                        Console.WriteLine("======  test 1 ========= ");
                        foreach (MethodDefinition m in methods)
                        {
                            Console.WriteLine("Method Name : {0}", m.GetFullName());

                        }
                       
                    }
                    finally
                    {
                        project.WorkingSet.ReleaseReadLock();
                    }


                }
              

                Console.ReadLine();
                Console.WriteLine("print hello");

            }
            TimeSpan ts = DateTime.Now - dt;
            Console.WriteLine(ts.ToString());
            Console.ReadLine();
        }


        private class Options {
            [VerbOption("callgraph", HelpText = "Analyze stereotype of methods in the project")]
            public CallgraphOptions StereotypeVerb { get; set; }

            [VerbOption("testcases", HelpText = "find all test cases in a project")]
            public TestCaseDetectOptions FindAllTestCaseVerb { get; set; }


            [VerbOption("summary", HelpText = "summarize test cases in a project")]
            public SummarizeTestOptions SummaryTestCaseVerb { get; set; }

            
            [VerbOption("hello", HelpText = "Print hello for testing")]
            public HelloOptions HelloVerb { get; set; }

            [HelpVerbOption]
            public string GetUsage(string verb) {
                return HelpText.AutoBuild(this, verb);
            }
        }




        /// <summary>
        /// Stereotype detector
        /// </summary>
        private class CallgraphOptions {
            /// <summary> Subject application location </summary>
            [Option("loc", Required = true, HelpText = "The subject project folder")]
            public string LocationsPath { get; set; }

            [Option("srcmlPath", Required = true, HelpText = "The path to Srcml.exe")]
            public string SrcMLPath { get; set; }
        }



        /// <summary>
        /// Options for findAllTestCases
        /// </summary>
        private class TestCaseDetectOptions {
            /// <summary> Subject application location </summary>
            [Option("loc", Required = true, HelpText = "The subject project folder")]
            public string LocationsPath { get; set; }

            [Option("srcmlPath", Required = true, HelpText = "The path to Srcml.exe")]
            public string SrcMLPath { get; set; }
        }



        /// <summary>
        /// Stereotype detector
        /// </summary>
        private class SummarizeTestOptions {
            /// <summary> Subject application location </summary>
            [Option("loc", Required = true, HelpText = "The subject project folder")]
            public string LocationsPath { get; set; }

            [Option("srcmlPath", Required = true, HelpText = "The path to Srcml.exe")]
            public string SrcMLPath { get; set; }



            [Option("outputLoc", Required = true, HelpText = "Summary Output location")]
            public string OutputLoc { get; set; }
        }
      


        /// <summary>
        /// print hello for testing 
        /// </summary>
        private class HelloOptions {
        }
    }

}
