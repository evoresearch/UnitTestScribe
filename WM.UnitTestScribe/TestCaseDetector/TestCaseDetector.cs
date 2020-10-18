using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ABB.SrcML;
using ABB.SrcML.Data;
using System.Configuration;

namespace WM.UnitTestScribe.TestCaseDetector
{
    public class TestCaseDetector
    {
        static int i = 0;
        /// <summary> Subject application location </summary>
        public string LocalProj { get; private set; }


        /// <summary> SrcML directory location </summary>
        public string SrcmlLoc { get; private set; }


        /// <summary> Stores all test cases </summary>
        public readonly HashSet<TestCaseID> AllTestCases;


        /// <summary> The temp directory to store all xml </summary>
        readonly public string TempDir;
        private string annotationPattern;
        private string[] testCaseAnnotationNames;
        private string testAnnotations;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localProj"> The subject location</param>
        /// <param name="srcmlloc">Srcml executable file location</param>
        public TestCaseDetector(string localProj, string srcmlloc,string targetProject="source")
        {
            this.LocalProj = localProj;
            this.SrcmlLoc = srcmlloc;
            //create temp into 
            this.TempDir = Util.CreateTemporalDir(); //targetProject == "source"? Util.CreateTemporalDir(ConfigurationManager.AppSettings["srcMLTestCaseTempFolderSourceProject"]): Util.CreateTemporalDir(ConfigurationManager.AppSettings["srcMLTestCaseTempFolderTargetProject"]); 
            this.AllTestCases = new HashSet<TestCaseID>();
            annotationPattern = ConfigurationManager.AppSettings["testCaseAnnotationPattern"].ToString().ToLower();
            testCaseAnnotationNames = ConfigurationManager.AppSettings["testCaseAnnotationNames"].ToString().Split(',');
            testAnnotations = ConfigurationManager.AppSettings["testCaseAnnotationNames"];
        }



        /// <summary>
        /// Analyzes the project.
        /// </summary>
        public void AnalyzeTestCases()
        {

            //generate project xml files under tempPath
            Src2XML sx = new Src2XML();
            Console.WriteLine("Run SrcML. generating XML files for project {0}... ",LocalProj);
            sx.SourceFolderToXml(this.LocalProj, this.TempDir, this.SrcmlLoc);
            Console.WriteLine("generating XML files is done.");

            //Now, TempDir contains all cs files for the project in xml version 

            Console.WriteLine("Parse XML files {0}... ", LocalProj);

            foreach (string fileName in Directory.EnumerateFiles(TempDir, "*.xml*", SearchOption.AllDirectories))
            {
                try
                {
                    ParseXMLFile(fileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("parse xml file error ");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.InnerException!=null?ex.InnerException.Message:"");
                    Console.WriteLine(ex.StackTrace);
                }
            }
            //foreach (var fileName in Directory.GetFiles(this.TempDir))
            //{
            //    if (fileName.EndsWith(".xml"))
            //    {
            //        try
            //        {
            //            ParseXMLFile(fileName);
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine("parse xml file error ");
            //            Console.WriteLine(ex.StackTrace);
            //        }
            //    }
            //}
            //Directory.Delete(TempDir, true);
            Console.WriteLine("done! ");
        }



        /// <summary>
        /// Parses the file and extracts all testcaseID.
        /// </summary>
        /// <param name="fileName"></param>
        private void ParseXMLFile(string fileName)
        {
            do
            {

            } while (Util.IsFileInUse(fileName));

            //Console.WriteLine("File {1} exists: {0}", File.Exists(fileName),fileName);
            //Console.WriteLine("File is text is: {0}", File.ReadAllText(fileName));
            XElement elementRoot = XElement.Load(fileName);
            var funcElements = elementRoot.DescendantsAndSelf().Where(e => e.Name == SRC.Function);

            foreach (var func in funcElements)
            {
                if (IsTestCase(func))
                {
                    string functionName = GetNameForMethodOrClass(func);
                    var classElement = func.AncestorsAndSelf().FirstOrDefault(e => e.Name == SRC.Class);
                    var className = "";
                    if (classElement != null)
                        className = GetNameForMethodOrClass(classElement);

                    //noiced that it was difficult to catch package names in Java files by simly using SRC.Package
                    var nsElement = func.AncestorsAndSelf().FirstOrDefault(e => e.Name == SRC.Namespace || e.Name== SRC.Package || (e.FirstNode.ToString().Contains("<package")));
                    var nsName = "";
                    if (nsElement != null)
                    {
                        if (nsElement.ToString().Contains("package"))
                        {
                            nsName = nsElement.Element(SRC.Package).Element(SRC.Name).Value;
                        }
                        else
                        {
                            nsName = nsElement.Element(SRC.Name).Value;
                        }
                    }

                    AllTestCases.Add(new TestCaseID(nsName, className, functionName));
                    //Console.WriteLine(nsName + "  " + className + "  " + functionName);

                    i++;

                }
            }
            //Console.WriteLine("total  : " + i);
        }



        /// <summary>
        /// Gets method name for the given method or class
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetNameForMethodOrClass(XElement element)
        {
            if (element == null)
                return "";
            //throw new ArgumentNullException("element");
            //if (!MethodElementNames.Contains(element.Name) && !TypeElementNames.Contains(element.Name))
            //    throw new ArgumentException("must be a method element or a type element", "element");

            var nameElement = element.Element(SRC.Name);
            if (null == nameElement)
                return string.Empty;
            return NameHelper.GetLastName(nameElement);
        }



        /// <summary>
        /// Checks if the method is a test case.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private bool IsTestCase(XElement method)
        {
            var typeEle = method.Element(SRC.Type);
            if (typeEle == null)
                return false;

            var attributes = getElements(typeEle);            

            foreach (var attribute in attributes)
            {
                var names = attribute.DescendantsAndSelf().Where(e => e.Name == SRC.Name);
                foreach (var name in names)
                {
                    string curName = name.Value;
                    if (testAnnotations.Contains(curName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private IEnumerable<XElement> getElements(XElement typeElement)
        {
            
            switch (annotationPattern)
            {
                case "annotation":
                    return typeElement.Elements(SRC.Annotation);
                default:
                    return typeElement.Elements(SRC.Attribute);
            }

        }

     


    }
}
