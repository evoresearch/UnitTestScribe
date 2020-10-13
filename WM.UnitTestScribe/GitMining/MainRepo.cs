using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.GitMining
{
   public class MainRepo
    {
        public MainRepo(String repoName, String createdAt, double forks, double size)
        {
            this.repoName = repoName;
            this.createdAt = createdAt;
            this.forks = forks;
            this.size = size;
        }
        private String repoName, createdAt;
        private double forks, size;
        
        public string RepoName { get => repoName; set => repoName = value; }
        public string CreatedAt { get => createdAt; set => createdAt = value; }
        public double Forks { get => forks; set => forks = value; }
        public double Size { get => size; set => size = value; }
    }
}
