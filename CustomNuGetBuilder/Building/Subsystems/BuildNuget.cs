using CoreFramework.Base.IO;
using CustomNuGetBuilder.Building.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CustomNuGetBuilder.Building.Subsystems
{
   public class BuildNuget
   {
      private BuildConfig Config { get => Builder.Config; }

      private BuildNugetConfig SpecConfig { get => Config.BuildNugetConfig;  }

      public Build Builder { get; set; }

      public BuildNuget()
      {
      }

      public void Build()
      {
         if (!File.Exists(SpecConfig.ExePath))
         {
            Log.Info($"Can't find {nameof(SpecConfig.ExePath)}");
            Log.Info($"Downloading '{SpecConfig.ExeDownloadPath}' to '{SpecConfig.ExePath}'");

            using (WebClient dwl = new WebClient())
            {
               dwl.DownloadFile(SpecConfig.ExeDownloadPath, SpecConfig.ExePath);
            }

            Log.Info("Download done!");
         }

         if (SpecConfig.AutoUpdate)
         {
            Log.Info("Trying to update nuget.exe ...");
            Log.Info($"Starting '{SpecConfig.ExePath}'... Redirecting ouput streams...");

            Process pupd = Process.Start(new ProcessStartInfo
            {
               UseShellExecute = false,
               RedirectStandardError = true,
               RedirectStandardOutput = true,
               FileName = SpecConfig.ExePath,
               Arguments = @"update -self"
            });
            pupd.BeginErrorReadLine();
            pupd.BeginOutputReadLine();

            pupd.OutputDataReceived += NuGetODataReceived;
            pupd.ErrorDataReceived += NuGetEDataReceived;

            pupd.WaitForExit();

            Log.Info("Autoupdate done!");
         }


         Log.Info($"Trying to build a nuget package...");
         Log.Info($"{nameof(SpecConfig.ExecCommand)}='{SpecConfig.ExecCommand}'");

         Log.Info($"Resolving VS-BuildParameters for command...");

         string nugetexeccmd = Builder.ReplaceWithPara(SpecConfig.ExecCommand).Replace("nuget", "").Trim();
         if (nugetexeccmd.Contains("-outputdirectory"))
         {
            Log.Error($"-outputdirectory is specified by special structure in the config file! Check '{Builder.CmdOption.ConfigPath}'");
            return;
         }

         if (!Directory.Exists(SpecConfig.LatestBuildDir))
            Directory.CreateDirectory(SpecConfig.LatestBuildDir);

         if (SpecConfig.ArchiveBuildDir != null)
         {
            if (!Directory.Exists(SpecConfig.ArchiveBuildDir))
               Directory.CreateDirectory(SpecConfig.ArchiveBuildDir);

            Log.Info($"Copying everything '{SpecConfig.LatestBuildDir}'-->'{SpecConfig.ArchiveBuildDir}'");

            DirUtil.Copy(SpecConfig.LatestBuildDir, SpecConfig.ArchiveBuildDir);
         }

         DirUtil.DeleteSafe(SpecConfig.LatestBuildDir);

         nugetexeccmd += $" -outputdirectory \"{SpecConfig.LatestBuildDir}\"";


         Log.Info($"Executing '{SpecConfig.ExePath}' with '{nugetexeccmd}'");

         Process p = Process.Start(new ProcessStartInfo
         {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = SpecConfig.ExePath,
            Arguments = nugetexeccmd,
         });
         p.BeginErrorReadLine();
         p.BeginOutputReadLine();

         p.OutputDataReceived += NuGetODataReceived;
         p.ErrorDataReceived += NuGetEDataReceived;

         p.WaitForExit();

         Log.Info("Building done!");
      }

      private void NuGetODataReceived(object sender, DataReceivedEventArgs e)
      {
         if (e.Data != null)
            Log.Info(e.Data);
      }

      private void NuGetEDataReceived(object sender, DataReceivedEventArgs e)
      {
         if (e.Data != null)
            Log.Error(e.Data);
      }
   }
}
