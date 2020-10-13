using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeaCap.GitMining
{
    public class ProjectCloner
    {
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
            foreach (ForkedRepo repo in forkedRepos)
            {
                if (!projects.Contains(repo.ParentRepo))
                {
                    projects.Add(repo.ParentRepo);
                }
                projects.Add(repo.ForkName);
            }
            //create list of project names and clone urls
            Dictionary<string, string> projectURLs = new Dictionary<string, string>();
            foreach (string project in projects)
            {
                PullRequest projectPullRequest = pullRequests.FirstOrDefault(p => (p.SourceRepo == project || p.TargetRepo == project)&!string.IsNullOrWhiteSpace(p.MergeCommitHash));
                if (projectPullRequest != null)
                {
                    projectURLs.Add(project, projectPullRequest.TargetCloneURL);
                }
            }

            //now clone repos
            string folderName = ConfigurationManager.AppSettings["clonedProjects"];
            foreach (string project in projectURLs.Keys)
            {
                Console.WriteLine($"cloning {project}");
                try
                {
                    string projectSHortName = project.Split('/')[1];
                    string projectFolder = string.Format($"{folderName}\\{projectSHortName}");
                    string cloneURL;
                    projectURLs.TryGetValue(project, out cloneURL);
                    Repository.Clone(cloneURL,projectFolder);
                    break;
                    

                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message+"\n"+ex.StackTrace);
                }
            }
        }

    }
}
