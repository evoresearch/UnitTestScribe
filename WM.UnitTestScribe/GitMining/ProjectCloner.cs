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
            setPullRequests();

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
            Dictionary<string, string> projectURLs = new Dictionary<string, string>();
            foreach (string project in projects)
            {
                PullRequest projectPullRequest = pullRequests.FirstOrDefault(p => (p.SourceRepo == project || p.TargetRepo == project) && p.TargetCloneURL.Contains(project) && !string.IsNullOrWhiteSpace(p.MergeCommitHash));
                if (projectPullRequest != null)
                {
                    projectURLs.Add(project, projectPullRequest.TargetCloneURL);
                }
            }

            //check if commands includes detecting test cases


            //now clone repos
            string folderName = ConfigurationManager.AppSettings["clonedProjects"];
            string commands = ConfigurationManager.AppSettings["commands"];
            string clonedProjectsFolder = ConfigurationManager.AppSettings["clonedProjects"];
            string outputFile = string.Format($"{clonedProjectsFolder}\\testCases_allprojects.csv");
            if (commands.ToLower().Contains("testdetect"))
            {

                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            }
            StreamWriter writer = File.AppendText(outputFile);
            writer.WriteLine("project;parent;projectIsFork;cloneURL;package;className;methodName");
            try
            {
                foreach (string project in projectURLs.Keys)
                {
                    string projectSHortName = project.Split('/')[1];
                    string projectFolder = string.Format($"{folderName}\\{project.Replace("/", "\\")}");
                    string cloneURL;
                    projectURLs.TryGetValue(project, out cloneURL);
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

                            Console.WriteLine($"Detecing test cases in {project}");
                            TestCaseDetector testCaseDetector = new TestCaseDetector(projectFolder, srcMLLocation);
                            testCaseDetector.AnalyzeTestCases();
                            HashSet<TestCaseID> allTestCases = testCaseDetector.AllTestCases;

                            //projct parent
                            ForkedRepo repo = forkedRepos.FirstOrDefault(f => f.ForkName == project);
                            string parent = repo == null ? "null" : repo.ParentRepo;
                            bool isFork = repo != null;




                            foreach (TestCaseID testCase in allTestCases)
                            {
                                writer.WriteLine($"{project};{parent};{isFork};{cloneURL};{testCase.NamespaceName};{testCase.ClassName};{testCase.MethodName}");
                                writer.Flush();
                                Console.WriteLine($"Found {testCase.NamespaceName}.{testCase.ClassName}.{testCase.MethodName}");
                            }

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
                writer.Close();
            }
        }

    }
}
