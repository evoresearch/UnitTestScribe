using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.GitMining
{
   public class ForkedRepo
    {
        public ForkedRepo(String parentRepo)
        {
            this.parentRepo = parentRepo;
        }

        public ForkedRepo(String parentRepo, String repoDate, String forkName, String createdAt, String firstCommitDate, String lastCommitDate, double totalCommits)
        {
            this.parentRepo = parentRepo;
            this.repoDate = repoDate;
            this.forkName = forkName;
            this.createdAt = createdAt;
            this.firstCommitDate = firstCommitDate;
            this.lastCommitDate = lastCommitDate;
            this.totalCommits = totalCommits;
        }
        private string parentRepo, repoDate, forkName, createdAt, firstCommitDate, lastCommitDate;
        private double totalCommits;

        public string ParentRepo { get => parentRepo; set => parentRepo = value; }
        public string RepoDate { get => repoDate; set => repoDate = value; }
        public string ForkName { get => forkName; set => forkName = value; }
        public string CreatedAt { get => createdAt; set => createdAt = value; }
        public string FirstCommitDate { get => firstCommitDate; set => firstCommitDate = value; }
        public string LastCommitDate { get => lastCommitDate; set => lastCommitDate = value; }
        public double TotalCommits { get => totalCommits; set => totalCommits = value; }
    }
}
