using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNuGetBuilder.Building.Config
{
   public class GitHubReleaseConfig
   {
      public bool Enabled { get; set; } = false;

      public string GitHubPersonalAccessToken { get; set; } = "";

      /// <summary>
      /// null = we are in repo (Workdir!)
      /// </summary>
      public string LocalRepoPath { get; set; } = null;

      public bool CheckIfLocalRepoIsDirty { get; set; } = false;

      public string RemoteHead { get; set; } = "origin";

      public List<string> AllowedReleaseBranches { get; set; } = null;

      public bool IsPreRealease { get; set; } = true;

      public bool IsDraft { get; set; } = true;

      public TimeSpan? AssetUploadTimeOut { get; set; } = TimeSpan.FromMinutes(5);

   }
}
