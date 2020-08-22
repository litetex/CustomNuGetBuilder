using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNuGetBuilder.Building.Config
{
   public class BuildNugetConfig
   {
      public string ExeDownloadPath { get; set; } = @"https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";

      public string ExePath { get; set; } = "nuget.exe";

      public bool AutoUpdate { get; set; } = true;

      public string ExecCommand { get; set; } = "nuget pack \"$(ProjectPath)\" -Properties Configuration=$(ConfigurationName) -IncludeReferencedProjects -Verbosity detailed -ForceEnglishOutput";

      public string LatestBuildDir { get; set; } = Path.Combine("builds", "latest");

      /// <summary>
      /// null = ignore and delete everything that was in latest
      /// </summary>
      public string ArchiveBuildDir { get; set; } = Path.Combine("builds", "archiv");
   }
}
