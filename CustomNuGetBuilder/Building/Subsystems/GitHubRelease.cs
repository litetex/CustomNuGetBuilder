using CustomNuGetBuilder.Building.Config;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNuGetBuilder.Building.Subsystems
{
   public class GitHubRelease
   {
      private BuildConfig Config { get => Builder.Config; }

      private GitHubReleaseConfig SpecConfig { get => Config.GitHubReleaseConfig; }

      public Build Builder { get; set; }

      public GitHubRelease()
      {

      }

      public void Release()
      {
         try
         {
            //find file version - use file in latest
            Log.Info("Trying to create a new release...");
            FileInfo tarfile;

            string latestbuilddir = Config.BuildNugetConfig.LatestBuildDir;

            var latestfiles = new DirectoryInfo(latestbuilddir).GetFiles().Where(x => x.Extension == ".nupkg");
            if (!latestfiles.Any())
               throw new FileNotFoundException($"Could not find any file in '{latestbuilddir}'");
            else if (latestfiles.Count() > 1)
            {
               Log.Warn($"Found multiple files in '{latestbuilddir}'! Choosing latest...");

               var maxdate = latestfiles.Max(x => x.CreationTimeUtc);
               tarfile = latestfiles.First(x => x.CreationTimeUtc == maxdate);
            }
            else
               tarfile = latestfiles.First();

            string namewithoutext = Path.GetFileNameWithoutExtension(tarfile.Name);
            string version = "v" + namewithoutext.Substring(namewithoutext.IndexOf('.') + 1);

            Log.Info($"Using version='{version}' derived from '{tarfile.Name}'");

            CredentialsHandler gitcreditshandler = new CredentialsHandler(
                   (url, usernameFromUrl, types) =>
                       new UsernamePasswordCredentials()
                       {
                          Username = SpecConfig.GitHubPersonalAccessToken,
                          Password = string.Empty
                       });

            string repopath = SpecConfig.LocalRepoPath ?? Directory.GetCurrentDirectory();

            Log.Info($"Trying to find corresponding repo in '{repopath}'");

            string tagobjectsha;
            string remoteownername;
            string remotereponame;
            string branchname;
            using (LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(LibGit2Sharp.Repository.Discover(repopath)))
            {
               Log.Info($"Found repo. Path={repo.Info.Path}");

               if (SpecConfig.CheckIfLocalRepoIsDirty && repo.RetrieveStatus().IsDirty)
                  throw new InvalidOperationException("Git directory contains uncommited contents");

               Log.Info($"Repo found trying to update/fetch from remote");
               FetchOptions options = new FetchOptions
               {
                  CredentialsProvider = gitcreditshandler
               };

               Remote localremote = repo.Network.Remotes[SpecConfig.RemoteHead];

               try
               {
                  Commands.Fetch(repo, localremote.Name, localremote.FetchRefSpecs.Select(x => x.Specification), options, "");
               }
               catch (Exception e)
               {
                  Log.Error($"Failed to update from remote[{localremote.Name}]: ", e);
                  throw;
               }

               Log.Info($"Local Repository Remote[{localremote.Name}] URL = '{localremote.Url}'");

               //e.g: https://github.com/username/reponame.git
               string[] remoteurlparts = localremote.Url.Remove(localremote.Url.Length - 4).Replace("https://github.com/", "").Split('/');
               // username reponame
               remoteownername = remoteurlparts[0];
               remotereponame = remoteurlparts[1];

               Log.Info($"Extracted {nameof(remoteownername)}='{remoteownername}' and {nameof(remotereponame)}='{remotereponame}'! Getting GitHub repo...");

               branchname = repo.Head.FriendlyName;

               if (SpecConfig.AllowedReleaseBranches != null && !SpecConfig.AllowedReleaseBranches.Contains(branchname))
               {
                  Log.Error($"Current Branch[{branchname}] is not allowed! see {nameof(SpecConfig.AllowedReleaseBranches)}");
                  throw new InvalidOperationException("Branch not allowed");
               }
               Log.Info($"Current branch is '{branchname}'");

               //Get remote branch and check if they are equal if not stop work
               LibGit2Sharp.Commit releasescommitpoint = repo.Head.Tip;
               if (!repo.Head.TrackedBranch.Commits.Contains(releasescommitpoint))
               {
                  Log.Error("The remote and the local branch are not synced!");
                  throw new InvalidOperationException("Remote and Local are not synced!");
               }

               tagobjectsha = releasescommitpoint.Id.Sha;
               Log.Info($"Found corresponding commit: message='{releasescommitpoint.MessageShort}' SHA:{tagobjectsha}");
            }

            Log.Info("Conneting to Github...");
            var client = new GitHubClient(new ProductHeaderValue("CustomNuGetBuilder"))
            {
               Credentials = new Octokit.Credentials(SpecConfig.GitHubPersonalAccessToken)
            };

            var miscellaneousRateLimit = client.Miscellaneous.GetRateLimits().Result;

            //  The "core" object provides your rate limit status except for the Search API.
            var coreRateLimit = miscellaneousRateLimit.Resources.Core;

            var CoreRequestsPerHour = coreRateLimit.Limit;
            var CoreRequestsLeft = coreRateLimit.Remaining;
            var CoreLimitResetTime = coreRateLimit.Reset; // UTC time
            Log.Info($"API RateLimit Info: {nameof(CoreRequestsPerHour)}={CoreRequestsPerHour}/{nameof(CoreRequestsLeft)}={CoreRequestsLeft}/{nameof(CoreLimitResetTime)}={CoreLimitResetTime}(UTC)");

            //create tags
            Log.Info($"Creating tag: Location='/{remoteownername}/{remotereponame}' Tag='{version}'");
            GitTag tag = client.Git.Tag.Create(remoteownername, remotereponame, new NewTag()
            {
               Message = $"Tag for {version}",
               Type = TaggedType.Commit,
               Tag = version,
               Tagger = new Committer(Config.Username, Config.Email, DateTimeOffset.Now),
               Object = tagobjectsha,
            }).Result;
            Log.Info($"Created tag: SHA='{tag.Sha}'");

            //create release
            Log.Info($"Creating release: Location='/{remoteownername}/{remotereponame}'");
            Release release = client.Repository.Release.Create(remoteownername, remotereponame, new NewRelease(version)
            {
               Draft = SpecConfig.IsDraft,
               Prerelease = SpecConfig.IsPreRealease,
               Name = version,
               Body = $"Release {version}; Created at {DateTimeOffset.Now} on branch['{branchname}'], TagObjectSHA={tagobjectsha}",
               //TargetCommitish = //maybe branch name
            }).Result;


            Log.Info($"Uploading nuget file[{tarfile.Name}] to release");

            using (var content = File.OpenRead(tarfile.FullName))
            {
               var assetUpload = new ReleaseAssetUpload()
               {
                  FileName = tarfile.Name,
                  RawData = content,
                  ContentType = "application/zip", //No idea what is the best for nuspec files
                  Timeout = SpecConfig.AssetUploadTimeOut,
               };

               ReleaseAsset ras = client.Repository.Release.UploadAsset(release, assetUpload).Result;

               Log.Info($"Asset uploaded: URL='{ras.BrowserDownloadUrl}' Size='{ras.Size}'");
            }

            Log.Info("Creating release finished!");
         }
         catch (Exception e)
         {
            Log.Error("Failed to generate GitHubRelease", e);
         }
      }

   }
}
