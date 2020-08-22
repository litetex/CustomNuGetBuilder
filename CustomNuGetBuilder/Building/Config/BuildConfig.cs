using CoreFramework.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNuGetBuilder.Building.Config
{
   public class BuildConfig : JsonConfig
   {
      public string Username { get; set; } = "CustomNuGetBuilder";

      public string Email { get; set; } = "example@example.example";

      public BuildNugetConfig BuildNugetConfig { get; set; } = new BuildNugetConfig();

      public AppendToGitRepoConfig AppendToGitRepoConfig { get; set; } = new AppendToGitRepoConfig();

      public GitHubReleaseConfig GitHubReleaseConfig { get; set; } = new GitHubReleaseConfig();

   }
}
