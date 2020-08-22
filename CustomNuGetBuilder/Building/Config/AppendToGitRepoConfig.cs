using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNuGetBuilder.Building.Config
{
   public class AppendToGitRepoConfig
   {
      /// <summary>
      /// false = disabled
      /// </summary>
      public bool Enabled { get; set; } = false;

      /// <summary>
      /// Username / Token for repo
      /// </summary>
      public string CredsPrincipal { get; set; } = null;

      /// <summary>
      /// Password (optional) for repo
      /// </summary>
      public string CredsPassword { get; set; } = null;

      public string LocalGitDir { get; set; } = Path.Combine("git", "repo");

      /// <summary>
      /// null = ignore
      /// </summary>
      public string RemoteGitPath { get; set; } = null;

   }
}
