using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.GitMining
{
   public class MiningUtility
    {
        /**
    * Gets top N repos from a list of lines typically read from a CSV file with the following columns
    * ID,	 ParentRepo,	 RepoDate,	 Fork,	 TotalCommits,	 CreatedAt,	 FirstCommitDate,	 LastCommitDate,	repoPairID
    * @param lines
    * @return
    */
        public static List<ForkedRepo> getForkedRepos(List<String> lines)
        {

            List<ForkedRepo> forkedRepos = new List<ForkedRepo>();
            foreach (String line in lines)
            {
                String[] items = line.Split(';');
                if (!items[1].Contains("/"))
                {
                    continue;//skip the header row which has no actual projects
                }
                forkedRepos.Add(new ForkedRepo(items[1].Trim(), items[2].Trim(), items[3].Trim(), items[5].Trim(), items[6].Trim(), items[7].Trim(), Convert.ToDouble(items[4].Trim())));

            }
            return forkedRepos;
        }

        /**
         * Gets main repos from a list of lines read from a CSv file with the following columns
         * Repos_Name (full),	 Name(short),	 Created_at, login,	 Forks,	 Size,	 Tags,	 Language,	 Description
         * @param lines
         * @return
         */
        public static List<MainRepo> getMainRepos(List<String> lines)
        {
            List<MainRepo> mainRepos = new List<MainRepo>();
            foreach (String line in lines)
            {
                String[] items = line.Split(';');
                if (!items[0].Contains("/"))
                {
                    continue;//skip the header row which has no actual projects
                }
                mainRepos.Add(new MainRepo(items[0].Trim(), items[2].Trim(), Convert.ToDouble(items[4].Trim()), Convert.ToDouble(items[5].Trim())));

            }
            return mainRepos;
        }

        /**
         * Gets a pull requests from a list of lines typically read from a CSV file
         * sourceRepo,	sourceRepoIsFork,	targetRepo,	targetRepoIsFork,	state,	createdAt,	updatedAt,	closedAt,	mergedAt,	mergeCommitHash,	number
         * @param lines
         * @return
         */
        public static List<PullRequest> getPullRequests(List<String> lines)
        {
            List<PullRequest> pullRequests = new List<PullRequest>();
            foreach (String line in lines)
            {
                String[] items = line.Split(';');
                if (!items[0].Contains("/"))
                {//skip null source repos
                    continue;//skip the header row which has no actual projects
                }
                pullRequests.Add(new PullRequest(items[0], Boolean.Parse(items[1]), items[2], Boolean.Parse(items[3]), items[4], items[5], items[6], items[7], items[8], items[9], Convert.ToInt64(items[10]),items[11],items[12]));

            }
            return pullRequests;
        }
    }
}
