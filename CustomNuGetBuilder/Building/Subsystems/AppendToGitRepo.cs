using CustomNuGetBuilder.Building.Config;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNuGetBuilder.Building.Subsystems
{
   public class AppendToGitRepo
   {
      private BuildConfig Config { get => Builder.Config; }

      private AppendToGitRepoConfig SpecConfig { get => Config.AppendToGitRepoConfig; }

      public Build Builder { get; set; }

      public AppendToGitRepo()
      {

      }

      public void Append()
      {
         CredentialsHandler gitcreditshandler = SpecConfig.RemoteGitPath != null ?
            new CredentialsHandler(
                (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                       Username = SpecConfig.CredsPrincipal,
                       Password = SpecConfig.CredsPassword ?? string.Empty
                    })
            : null;

         Log.Info("Appending build results to a repo");
         Log.Info($"Using local gitrepo='{SpecConfig.LocalGitDir}'");

         if (SpecConfig.RemoteGitPath != null)
            Log.Info($"Using remote gitrepo='{SpecConfig.RemoteGitPath}'");
         else
            Log.Info("No remote gitrepo specified");

         string localrepoloc = Repository.Discover(SpecConfig.LocalGitDir);
         if (localrepoloc == null)
         {

            Log.Info("No local git repo found...");
            if (SpecConfig.RemoteGitPath != null)
            {
               Log.Info("Cloning from remote");
               localrepoloc = Repository.Clone(SpecConfig.RemoteGitPath, SpecConfig.LocalGitDir, new CloneOptions
               {
                  CredentialsProvider = gitcreditshandler
               });
            }
            else
            {
               Log.Info("Creating bare repo");
               Directory.CreateDirectory(SpecConfig.LocalGitDir);
               localrepoloc = Repository.Discover(Repository.Init(SpecConfig.LocalGitDir));
            }
         }


         Identity id = new Identity(Config.Username, Config.Email);
         Log.Info($"Using Identity: Username='{Config.Username}', Email='{Config.Email}'");

         using Repository repo = new Repository(localrepoloc);
         var signature = new Signature(id, DateTimeOffset.Now);

         if (SpecConfig.RemoteGitPath != null)
         {
            Log.Info($"Pulling from remote");
            // Pull
            Commands.Pull(repo, signature, new PullOptions
            {
               FetchOptions = new FetchOptions
               {
                  CredentialsProvider = gitcreditshandler
               }
            });
         }

         Log.Info("Copying files from latest builds into repo...");
         string nugetgitprojectpath = Path.Combine(SpecConfig.LocalGitDir);
         if (!Directory.Exists(nugetgitprojectpath))
            Directory.CreateDirectory(nugetgitprojectpath);

         foreach (FileInfo f in new DirectoryInfo(Config.BuildNugetConfig.LatestBuildDir).GetFiles())
         {
            Log.Info($"Copy: '{f.FullName}'-->'{Path.Combine(nugetgitprojectpath, f.Name)}'");
            File.Copy(f.FullName, Path.Combine(nugetgitprojectpath, f.Name), true);
         }

         Log.Info("Adding all files to index");
         Commands.Stage(repo, "*");

         Signature sig = new Signature(id, DateTimeOffset.Now);

         Log.Info("Adding commit...");
         repo.Commit(
            $"Program: {AppDomain.CurrentDomain.FriendlyName} \r\n" +
            $"Job: {Builder.VSParsOptions.ProjectName}/{Builder.VSParsOptions.ConfigurationName}",
            sig, sig, new CommitOptions());

         if (SpecConfig.RemoteGitPath != null)
         {
            Log.Info("Pushing to remote...");
            repo.Network.Push(repo.Branches["master"], new PushOptions()
            {
               CredentialsProvider = gitcreditshandler
            });
            Log.Info("Appending into git repo done!");
         }
      }
   }
}
