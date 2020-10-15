using ABB.SrcML.Data.Queries;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeaCap.Propagation;
using WM.UnitTestScribe.Summary;
using WM.UnitTestScribe.TestCaseDetector;

namespace TeaCap.GitMining
{
    public class ProjectCloner
    {
        public ProjectCloner(string srcMLLocation)
        {
            this.srcMLLocation = srcMLLocation;

        }
        private string srcMLLocation;
        string pullRequestsFile = ConfigurationManager.AppSettings["pullRequestsFile"];
        string topNForksFile = ConfigurationManager.AppSettings["topNForksFile"];
        private List<ForkedRepo> forkedRepos;
        private List<PullRequest> pullRequests;
        
        public void setForkedRepos()
        {
            forkedRepos = MiningUtility.getForkedRepos(File.ReadAllLines(topNForksFile).ToList());
        }
        public void setPullRequests()
        {
            pullRequests = MiningUtility.getPullRequests(File.ReadAllLines(pullRequestsFile).ToList());
        }

        public void cloneRepos()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            setForkedRepos();
            //setPullRequests();

            List<string> projects = new List<string>();
            List<string> parents = new List<string>();
            foreach (ForkedRepo repo in forkedRepos)
            {
                if (!projects.Contains(repo.ParentRepo))
                {
                    projects.Add(repo.ParentRepo);
                    parents.Add(repo.ParentRepo);
                }
                projects.Add(repo.ForkName);
            }
            //create list of project names and clone urls
            //Dictionary<string, string> projectURLs = new Dictionary<string, string>();
            //foreach (string project in projects)
            //{

            //    //PullRequest projectPullRequest = pullRequests.FirstOrDefault(p => p.TargetCloneURL.Contains(project)|| p.SourceCloneURL.Contains(project));
            //    string cloneURL = string.Format($"https://github.com/{project}.git");
              
            //}

            //check if commands includes detecting test cases


            //now clone repos
            string folderName = ConfigurationManager.AppSettings["clonedProjects"];
            string commands = ConfigurationManager.AppSettings["commands"];
            string clonedProjectsFolder = ConfigurationManager.AppSettings["clonedProjects"];
            string testCasesFile = ConfigurationManager.AppSettings["testsAndUUTFile"];
            string classesAndMethodsFile = ConfigurationManager.AppSettings["classesAndMethodsFile"];

            if (commands.ToLower().Contains("testdetect"))
            {
                //test cases file
                if (File.Exists(testCasesFile))
                {
                    File.Delete(testCasesFile);
                }
                StreamWriter writer = File.AppendText(testCasesFile);
                writer.WriteLine("project;parent;projectIsFork;testPackage;testClassName;testMethodName;testMethodParameters;uutPackage;uutClassName;uutMethodName;uutMethodParameters");
                writer.Flush();
                writer.Close();
                //all clases and methods file
                if (File.Exists(classesAndMethodsFile))
                {
                    File.Delete(classesAndMethodsFile);
                }
                writer = File.AppendText(classesAndMethodsFile);
                writer.WriteLine("project;parent;projectIsFork;package;className;methodName;methodParameters");
                writer.Flush();
                writer.Close();
            }
             
            try
            {
                foreach (string project in projects)
                {
                    //string projectSHortName = project.Split('/')[1];
                    string projectFolder = string.Format($"{folderName}\\{project.Replace("/", "\\")}");
                    string cloneURL = string.Format($"https://github.com/{project}.git");
                    //projectURLs.TryGetValue(project, out cloneURL);
                    //DirectoryInfo directoryInfo = new DirectoryInfo(projectFolder);
                    //if (directoryInfo.Exists)
                    //{
                    //    directoryInfo.Delete(true);
                    //}
                    if (commands.ToLower().Contains("clone"))
                    {
                        Console.WriteLine($"cloning {project}");
                        try
                        {


                            Repository.Clone(cloneURL, projectFolder);



                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                        }
                    }
                    if (commands.ToLower().Contains("testdetect"))
                    {
                        try
                        {
                            
                            TestCaseDetector testCaseDetector = new TestCaseDetector(projectFolder, srcMLLocation);
                            testCaseDetector.AnalyzeTestCases();
                            HashSet<TestCaseID> allTestCases = testCaseDetector.AllTestCases;
                            HashSet<TestCaseSummary> projectTestSummary = new HashSet<TestCaseSummary>();
                            //projct parent
                            ForkedRepo repo = forkedRepos.FirstOrDefault(f => f.ForkName == project);
                            string parent = repo == null ? "null" : repo.ParentRepo;
                            bool isFork = repo != null;
                            Utilities.analyzeProjectTestCases(projectFolder, allTestCases, projectTestSummary, srcMLLocation, testCasesFile, classesAndMethodsFile, project, parent, isFork);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                        }

                    }
                }
            }catch(Exception ex)
        {
                Console.WriteLine(ex.StackTrace);
            }finally
            {
                
            }
        }

    }
}
