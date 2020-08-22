using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomNuGetBuilder.Building.Config;
using System.Net;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using CustomNuGetBuilder.Building.Subsystems;
using CustomNuGetBuilder.CmdOptions;
using Newtonsoft.Json;
using Shared;

namespace CustomNuGetBuilder.Building
{
   public class Build
   {
      public BuildConfig Config { get; set; }

      public CmdOption CmdOption { get; set; }

      public VSParsOption VSParsOptions { get; set; }

      private Dictionary<string, string> CmdParaDict { get; set; } = new Dictionary<string, string>();

      public Build(CmdOption cmdOpts, BuildConfig config)
      {
         CmdOption = cmdOpts;
         Config = config;

         //Decode BASE64
         byte[] data = Convert.FromBase64String(cmdOpts.Base64JsonPars);
         string decodedString = Encoding.UTF8.GetString(data);

         //Convert2JSON
         VSParsOptions = JsonConvert.DeserializeObject<VSParsOption>(decodedString);

         foreach (var pinf in typeof(VSParsOption).GetProperties())
            CmdParaDict.Add($"$({pinf.Name})", pinf.GetValue(VSParsOptions).ToString());
      }

      public void Execute()
      {
         new BuildNuget()
         {
            Builder = this,
         }.Build();

         Log.Info($"{nameof(Config.AppendToGitRepoConfig)}.{nameof(Config.AppendToGitRepoConfig.Enabled)}={Config.AppendToGitRepoConfig.Enabled}");
         if (Config.AppendToGitRepoConfig.Enabled)
            new AppendToGitRepo()
            {
               Builder = this,
            }.Append();

         Log.Info($"{nameof(Config.GitHubReleaseConfig)}.{nameof(Config.GitHubReleaseConfig.Enabled)}={Config.GitHubReleaseConfig.Enabled}");
         if (Config.GitHubReleaseConfig.Enabled)
            new GitHubRelease()
            {
               Builder = this
            }.Release();

         Log.Info("Done.");
      }

      public string ReplaceWithPara(string input)
      {
         foreach (var e in CmdParaDict)
            input = input.Replace(e.Key, e.Value);

         return input;
      }
   }
}
