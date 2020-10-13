using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.GitMining
{
   public class PullRequest
    {
        public PullRequest(String sourceRepo, bool sourceRepoIsFork, String targetRepo, bool targetRepoIsFork, String state, String createdAt, String updatedAt, String closedAt, String mergedAt, String mergeCommitHash, long number,string sourceCloneURL,string targetCloneURL)
        {
            this.sourceRepo = sourceRepo;
            this.sourceRepoIsFork = sourceRepoIsFork;
            this.targetRepo = targetRepo;
            this.targetRepoIsFork = targetRepoIsFork;
            this.state = state;
            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
            this.closedAt = closedAt;
            this.mergedAt = mergedAt;
            this.mergeCommitHash = mergeCommitHash;
            this.number = number;
            this.sourceCloneURL = sourceCloneURL;
            this.targetCloneURL = targetCloneURL;
        }
        private String repoName;
        private String sourceRepo;
        bool sourceRepoIsFork;
        private String targetRepo;
        private bool targetRepoIsFork;
        private String state;
        private String createdAt;
        private string updatedAt;
        private string closedAt;
        private String mergedAt;
        private String mergeCommitHash;
        private long number;
        private String sourceCloneURL, targetCloneURL;

        public string RepoName { get => repoName; set => repoName = value; }
        public string SourceRepo { get => sourceRepo; set => sourceRepo = value; }
        public bool SourceRepoIsFork { get => sourceRepoIsFork; set => sourceRepoIsFork = value; }
        public string TargetRepo { get => targetRepo; set => targetRepo = value; }
        public bool TargetRepoIsFork { get => targetRepoIsFork; set => targetRepoIsFork = value; }
        public string State { get => state; set => state = value; }
        public string CreatedAt { get => createdAt; set => createdAt = value; }
        public string UpdatedAt { get => updatedAt; set => updatedAt = value; }
        public string ClosedAt { get => closedAt; set => closedAt = value; }
        public string MergedAt { get => mergedAt; set => mergedAt = value; }
        public string MergeCommitHash { get => mergeCommitHash; set => mergeCommitHash = value; }
        public long Number { get => number; set => number = value; }
        public string SourceCloneURL { get => sourceCloneURL; set => sourceCloneURL = value; }
        public string TargetCloneURL { get => targetCloneURL; set => targetCloneURL = value; }
    }
}
