using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeaCap.DataAnalysis;

namespace TeaCap.GitMining
{
   public class DataAnalyzer
    {
        string testCasesFile = ConfigurationManager.AppSettings["testsAndUUTFile"];
        string classesAndMethodsFile = ConfigurationManager.AppSettings["classesAndMethodsFile"];
        string[] originalProjects = new string[] { "android/android-test", "athkalia/Just-Another-Android-App", "azhon/AppUpdate", "badoo/Chateau", "burgessjp/ThemeSkinning", "by-syk/NanoIconPack", "CUTR-at-USF/OpenTripPlanner-for-Android", "CyanogenMod/android_packages_apps_Bluetooth", "google/bundletool", "googlesamples/android-testdpc", "hypertrack/live-app-android", "iFixit/iFixitAndroid", "intrications/intent-intercept", "LineageOS/android_packages_apps_Contacts", "markusfisch/ShaderEditor", "Michenux/YourAppIdea", "nobunobuta/android_packages_apps_Browser", "nolanlawson/Catlog", "PatilShreyas/MaterialDialog-Android", "robotmedia/AndroidBillingLibrary", "segler-alex/RadioDroid", "segmentio/analytics-android", "smanikandan14/ThinDownloadManager", "vijayrawatsan/android-json-form-wizard", "woxingxiao/GracefulMovies", "xuexiangjys/TemplateAppProject" };
        List<TestCaseUUTPair> testCaseUUTPairs = new List<TestCaseUUTPair>();
        List<ProjectClassMethod> projectClassMethods = new List<ProjectClassMethod>();
        Dictionary<string, IEnumerable<TestCaseUUTPair>> projectTestPairs = null;//used to map a project to its list of testcaseUUT pairs
        List<TestCasePresence> testCasePresences = new List<TestCasePresence>();
        public DataAnalyzer()
        {
            GetTestCaseUUTPairs();
            GetProjectClassesAndMethods();
        }

        private TestCaseCount countTestCases(string project,string parent)
        {
            Console.WriteLine(project);
            TestCaseCount testCaseCount = new TestCaseCount();
            var testPairs = testCaseUUTPairs.Where(t => t.Project == project);
            projectTestPairs.Add(project, testPairs);
            var fullTestMethods = from t in testPairs
                                  select t.TestFullMethodName;
            var fullClasses = from t in testPairs
                                  select t.TestFullClassName;
            var fullUUTMethods = from t in testPairs
                                  select t.UutFullMethodName;
            var fullUUTClasses = from t in testPairs
                              select t.UutFullClassName;
            testCaseCount.Ecosystem = parent;
            testCaseCount.Project = project;
            testCaseCount.TestMethodCount = fullTestMethods.Distinct().Count();
            testCaseCount.TestClassCount = fullClasses.Distinct().Count();
            testCaseCount.UutMethodCount = fullUUTMethods.Distinct().Count();
            testCaseCount.UutClassCount = fullUUTClasses.Distinct().Count();

            return testCaseCount;

        }
       

        private void writeTestCaseCount(StreamWriter writer,TestCaseCount tcc)
        {
            writer.WriteLine($"{tcc.Ecosystem};{tcc.Project};{tcc.TestMethodCount};{tcc.TestClassCount};{tcc.UutMethodCount};{tcc.UutClassCount}");
            writer.Flush();
        }
        private void deleteExistingFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        public void CountEcoSystems()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            string parentFolder = Directory.GetParent(testCasesFile).FullName;
            string testCaseCountsFile = string.Format($"{parentFolder}\\projectsTestCaseCountDetail.csv");
            string ecosystemTestCaseCountsFile = string.Format($"{parentFolder}\\ecosystemTestCaseCountDetail.csv");
            string ecosystemTestCasePresenceFile = string.Format($"{parentFolder}\\ecosystemTestCasePresence.csv");
            string ecosystemTestCasePresenceSummaryFile = string.Format($"{parentFolder}\\ecosystemTestCasePresenceSummary.csv");
            string ecosystemTestCasePercentageSummaryFile = string.Format($"{parentFolder}\\ecosystemTestCasePercentageSummary.csv");
            string[] finalProjects = File.ReadAllLines(@"C:\testpropagation\clones\finalProjects.txt");

            //count how many test cases are in project
            deleteExistingFile(testCaseCountsFile);
            StreamWriter writer = File.AppendText(testCaseCountsFile);
            writer.WriteLine("ecosystem;project;testCaseCount;testClassCount;uutMethodCount;uutClassCount");

            //test case presence
            deleteExistingFile(ecosystemTestCasePresenceFile);
            StreamWriter presenceWriter = File.AppendText(ecosystemTestCasePresenceFile);
            presenceWriter.WriteLine("ecosystem;project;testcaseFullName;presence");

            //test case presence summary
            deleteExistingFile(ecosystemTestCasePresenceSummaryFile);
            StreamWriter presenceSummaryWriter = File.AppendText(ecosystemTestCasePresenceSummaryFile);
            presenceSummaryWriter.WriteLine("ecosystem;presence;q25;q50;q75;q100");

            //test case percentagfe summary (long list per quritile)
            deleteExistingFile(ecosystemTestCasePercentageSummaryFile);
            StreamWriter percentageSummaryWriter = File.AppendText(ecosystemTestCasePercentageSummaryFile);
            percentageSummaryWriter.WriteLine("ecosystem;presence;quartile;value");
            //first get all parent projects with children
            var parents = from parent in testCaseUUTPairs.Where(p=>finalProjects.Contains(p.Project))
                                   select parent.Parent;
             parents = parents.Distinct();

            double[] quartiles = new double[] { 0.25, 0.50, 0.75, 1 };

            foreach(string parent in parents)
            {
                projectTestPairs = new Dictionary<string, IEnumerable<TestCaseUUTPair>>();
                if (parent == "null"){
                    continue;
                }
                //do for parent
                TestCaseCount tcc = countTestCases(parent, parent);
                writeTestCaseCount(writer, tcc);
                //now do for children
                var forks = from pair in testCaseUUTPairs.Where(p => finalProjects.Contains(p.Project) && p.Parent == parent)
                            select pair.Project;
                forks = forks.Distinct();

                foreach(string fork in forks)
                {
                    tcc = countTestCases(fork, parent);
                    writeTestCaseCount(writer, tcc);
                }

                //get a map of tets pairs for each project and fork
                generateTestCasePresence(parent,presenceWriter);

                //get numbers for each qurtile
                PresenceQuartile presenceQuartile = new PresenceQuartile(parent);
                var ecosystemTestPresence = testCasePresences.Where(t => t.Ecosystem == parent);
                int projectCount = forks.Count()+1;//forks plus main project
                var testCases = from testPresence in ecosystemTestPresence
                                select testPresence.TestCaseFullName;
                testCases = testCases.Distinct();
                //skip those with less than 10 test cases
                if (testCases.Count() < 10)
                {
                    continue;
                }
                foreach(string testCase in testCases)
                {
                    //count projects in which it is present
                    var testCasePresence = ecosystemTestPresence.Where(p => p.TestCaseFullName == testCase);
                    var presentProjects = from tcp in testCasePresence.Where(p => p.Presence == PresenceType.Present)
                                          select tcp.Project;
                    presentProjects = presentProjects.Distinct();
                    var missingProjects = from tcp in testCasePresence.Where(p => p.Presence == PresenceType.Missing)
                                          select tcp.Project;
                    missingProjects = missingProjects.Distinct();


                    int presentIn = presentProjects.Count();
                    int missingIn = missingProjects.Count();
                    setPresenceCount(presenceQuartile, projectCount, presentIn, missingIn);

                }

                //get summary of counts
                printQuartileSummary(presenceSummaryWriter,percentageSummaryWriter, presenceQuartile);



            }
            writer.Close();
            presenceWriter.Close();
            presenceSummaryWriter.Close();
            percentageSummaryWriter.Close();
           


        }

        private void printQuartileSummary(StreamWriter presenceSummryWriter, StreamWriter percentageSummaryWriter, PresenceQuartile presenceQuartile)
        {
            //present
            double pq25 = presenceQuartile.P25.Sum();
            double pq50 = presenceQuartile.P50.Sum();
            double pq75 = presenceQuartile.P75.Sum();
            double pq100 = presenceQuartile.P100.Sum();
            writePresenceSummary(presenceSummryWriter, presenceQuartile.Ecosystem, PresenceType.Present, pq25, pq50, pq75, pq100);
            //missing
            double mq25 = presenceQuartile.M25.Sum();
            double mq50 = presenceQuartile.M50.Sum();
            double mq75 = presenceQuartile.M75.Sum();
            double mq100 = presenceQuartile.M100.Sum();
            writePresenceSummary(presenceSummryWriter, presenceQuartile.Ecosystem, PresenceType.Missing, mq25, mq50, mq75, mq100);

            //percentage
            double q25Sum = pq25 + mq25;double q50Sum = pq50 + mq50; double q75Sum = pq75 + mq75; double q100Sum = pq100 + mq100;
            double q25Present = pq25 / q25Sum, q25Missing = mq25 / q25Sum;
            double q50Present = pq50 / q50Sum, q50Missing = mq50 / q50Sum;
            double q75Present = pq75 / q75Sum, q75Missing = mq75 / q75Sum;
            double q100Present = pq25 / q25Sum, q100Missing = mq100 / q100Sum;
            writePercentageSummary(percentageSummaryWriter, presenceQuartile.Ecosystem, PresenceType.Present, "Q1-25%", q25Present);
            writePercentageSummary(percentageSummaryWriter, presenceQuartile.Ecosystem, PresenceType.Missing, "Q1-25%", q25Missing);
            writePercentageSummary(percentageSummaryWriter, presenceQuartile.Ecosystem, PresenceType.Present, "Q2-50%", q50Present);
            writePercentageSummary(percentageSummaryWriter, presenceQuartile.Ecosystem, PresenceType.Missing, "Q2-50%", q50Missing);
            writePercentageSummary(percentageSummaryWriter, presenceQuartile.Ecosystem, PresenceType.Present, "Q3-75%", q75Present);
            writePercentageSummary(percentageSummaryWriter, presenceQuartile.Ecosystem, PresenceType.Missing, "Q3-75%", q75Missing);
            writePercentageSummary(percentageSummaryWriter, presenceQuartile.Ecosystem, PresenceType.Present, "Q4-100%", q100Present);
            writePercentageSummary(percentageSummaryWriter, presenceQuartile.Ecosystem, PresenceType.Missing, "Q4-100%", q100Missing);
        }
        private static void setPresenceCount(PresenceQuartile presenceQuartile, int projectCount, int presentIn, int missingIn)
        {
            //present
            int floor25 = (int)Math.Floor(0.25 * projectCount);
            int floor50 = (int)Math.Floor(0.5 * projectCount); ;
            int floor75 = (int)Math.Floor(0.75 * projectCount);
            
            if (presentIn!=0 && presentIn >= floor25 )//25% of projects
            {
                presenceQuartile.P25.Add(1);
            }
            if (presentIn != 0 && presentIn >= floor50)//50% of projects
            {
                presenceQuartile.P50.Add(1);
            }
            if (presentIn != 0 && presentIn >= floor75)//75%
            {
                presenceQuartile.P75.Add(1);
            }
            if (presentIn != 0 && presentIn >= projectCount)//100%
            {
                presenceQuartile.P100.Add(1);
            }
            //missing
            if (missingIn!=0 && missingIn >= floor25)//25% of projects
            {
                presenceQuartile.M25.Add(1);
            }
            if (missingIn != 0 && missingIn >= floor50)//50% of projects
            {
                presenceQuartile.M50.Add(1);
            }
            if (missingIn != 0 && missingIn >= floor75)//75%
            {
                presenceQuartile.M75.Add(1);
            }
            if (missingIn != 0 && missingIn >= projectCount)//100%
            {
                presenceQuartile.M100.Add(1);
            }
        }

        private void writePresence(StreamWriter writer, TestCasePresence tp)
        {
            testCasePresences.Add(tp);
            writer.WriteLine($"{tp.Ecosystem};{tp.Project};{tp.TestCaseFullName};{tp.Presence}");
            writer.Flush();
        }
        private void writePresenceSummary(StreamWriter writer, string ecosystem,PresenceType presenceType, double q25, double q50, double q75, double q100)
        {
            
            writer.WriteLine($"{ecosystem};{presenceType};{q25};{q50};{q75};{q100}");
            writer.Flush();
        }
        private void writePercentageSummary(StreamWriter writer, string ecosystem, PresenceType presenceType, string quartile,double value)
        {
            if (Double.IsNaN(value)){
                value = 0;
            }
            writer.WriteLine($"{ecosystem};{presenceType};{quartile};{value}");
            writer.Flush();
        }
        private void generateTestCasePresence(string ecosytem, StreamWriter presenceWriter)
        {
            int projectPairsSize = projectTestPairs.Count();
            for (int i = 0; i < projectPairsSize; i++)
            {
                for (int j = i + 1; j < projectPairsSize; j++)
                {
                    string projectA = projectTestPairs.ElementAt(i).Key;
                    var a = from testCase in projectTestPairs.ElementAt(i).Value.Distinct()
                            select testCase.TestFullMethodName;
                    string projectB = projectTestPairs.ElementAt(j).Key;
                    var b = from testCase in projectTestPairs.ElementAt(j).Value.Distinct()
                            select testCase.TestFullMethodName;
                    var missingInB = a.Except(b);
                    var missingInA = b.Except(a);
                    var presentInBothAandB = a.Intersect(b);
                    //now add to list
                    foreach(string testCase in missingInA)
                    {                        
                        writePresence(presenceWriter, new TestCasePresence(ecosytem, projectA, testCase, PresenceType.Missing));
                    }
                    foreach (string testCase in missingInB)
                    {
                        writePresence(presenceWriter, new TestCasePresence(ecosytem, projectB, testCase, PresenceType.Missing));
                    }
                    foreach (string testCase in presentInBothAandB)
                    {
                        writePresence(presenceWriter, new TestCasePresence(ecosytem, projectA, testCase, PresenceType.Present));
                        writePresence(presenceWriter, new TestCasePresence(ecosytem, projectB, testCase, PresenceType.Present));
                    }

                }
            }
        }

        public void GetTestCaseUUTPairs()
        {
            string[] fileLines = File.ReadAllLines(testCasesFile);
            for(int line =1;line< fileLines.Length;line++)//start at 1 to exclude header
            {
                string[] items = fileLines[line].Split(';');
                TestCaseUUTPair pair = new TestCaseUUTPair();
                pair.Project = items[0];
                pair.Parent = items[1];
                pair.ProjectIsFork = Convert.ToBoolean(items[2]);
                pair.TestPackage = items[3];
                pair.TestClassName = items[4];
                pair.TestMethodName = items[5];
                pair.TestMethodParameters = items[6];
                pair.UutPackage = items[7];
                pair.UutClassName = items[8];
                pair.UutMethodName = items[9];
                pair.UutMethodParameters = items[10];
                testCaseUUTPairs.Add(pair);
            }
        }

        public void GetProjectClassesAndMethods()
        {
            string[] fileLines = File.ReadAllLines(classesAndMethodsFile);
            for (int line = 1; line < fileLines.Length; line++)//start at 1 to exclude header
            {
                string[] items = fileLines[line].Split(';');
                ProjectClassMethod pair = new ProjectClassMethod();
                pair.Project = items[0];
                pair.Parent = items[1];
                pair.ProjectIsFork = Convert.ToBoolean(items[2]);
                pair.Package = items[3];
                pair.ClassName = items[4];
                pair.MethodName = items[5];
                pair.MethodParameters = items[6];

                projectClassMethods.Add(pair);
            }
        }
    }
}
