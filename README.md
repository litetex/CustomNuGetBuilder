# CustomNuGetBuilder
:warning: This project got completely obsolete, when GitHub introduced [Package Registry](https://github.blog/2019-05-10-introducing-github-package-registry/) and [Actions](https://github.blog/2019-08-08-github-actions-now-supports-ci-cd/). It is now archived :warning:<br/>
:warning: The code is not validated

Creates nuget packages for projects and distributes them

## Setup

#### Requirements
 - Visual Studio (17+)

### Get the files
Download them. 

In the example a ```build``` folder is used in the repo

For the programs that are used:
 - VSPostBuildParameterCollector.exe → ```build/prog/VSPostBuildParameterCollector```
 - CustomNuGetBuilder.exe → ```build/prog/CustomNuGetBuilder```
   - lib/* → ```build/prog/CustomNuGetBuilder/lib/*```
   
### Generate the config
* Create a ```build/config``` folder
* Open a CLI and navigate into your ```build/prog/CustomNuGetBuilder``` folder<br/>
```cd <PathToYourVSSolutionLocation>/build/prog/CustomNuGetBuilder```
* Create a ```config.json```<br/>
```CustomNuGetBuilder.exe -genconf config.json```
* Copy the ```build/prog/CustomNuGetBuilder/config.json``` into ```build/config/config.json```

Now you should have something like that:<br/>
```config.json```
```JS
{
  "Username": "CustomNuGetBuilder", //Username that is used by the program e.g. BuildTool-<YourUsernam>
  "Email": "example@example.example", //Email that is uesd by the programm e.g. [github]@<YourUsername>
  "BuildNugetConfig": {
    "ExeDownloadPath": "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", //Update and download-location of nuget.exe
    "ExePath": "nuget.exe", //Path for Execution
    "AutoUpdate": true,
    "ExecCommand": "nuget pack \"$(ProjectPath)\" -Properties Configuration=$(ConfigurationName) -IncludeReferencedProjects -Verbosity detailed -ForceEnglishOutput", //Command to create the nugetfile
    "LatestBuildDir": "builds\\latest", //Output directory
    "ArchiveBuildDir": "builds\\archiv" //Archiv for old output files
  },
  "AppendToGitRepoConfig": {
    "Enabled": false, //true - Append created nuget-file into a git repo and push it
    "CredsPrincipal": null, // Username or Token
    "CredsPassword": null, // Password (if you use a token let this filed empty)
    "LocalGitDir": "git\\repo", //Localpath
    "RemoteGitPath": null //Remote path e.g. https://<hosturl>/<ownername>/<reponame>.git
  },
  "GitHubReleaseConfig": {
    "Enabled": false, //true - Automatically create a Github that contains the created files
    "GitHubPersonalAccessToken": "", // GitHub Personal Access Token
    "LocalRepoPath": null, //null - Program is executed inside a repo
    "CheckIfLocalRepoIsDirty": false, //Throws an exception if the (local) repo contains uncommited changes
    "RemoteHead": "origin", //Head that is used too connect to GitHub-Remote
    "AllowedReleaseBranches": null, //e.g. [ "master", "develop" ] --> if the current build doesn't occurs on one of the given branches, a exception will be thrown
    "IsPreRealease": true, //create the release as pre release
    "IsDraft": true, //create the release as draft
    "AssetUploadTimeOut": "00:05:00" //Max UploadTime per Asset - throws an Exception if it takes to long
  }
}
```

### Syntax for embedding and building automatically into VS-Studio:
Project-Properties\Buildevents\Postbuild
```
cd $(SolutionDir)build
for /f "delims=" %%i in ('prog\VSPostBuildParameterCollector\VSPostBuildParameterCollector.exe --OutDir "$(OutDir)\" --ConfigurationName "$(ConfigurationName)" --ProjectName "$(ProjectName)" --TargetName "$(TargetName)" --TargetPath "$(TargetPath)" --ProjectPath "$(ProjectPath)" --ProjectFileName "$(ProjectFileName)" --TargetExt "$(TargetExt)" --TargetFileName "$(TargetFileName)" --DevEnvDir "$(DevEnvDir)\" --TargetDir "$(TargetDir)\" --ProjectDir "$(ProjectDir)\" --SolutionFileName "$(SolutionFileName)" --SolutionPath "$(SolutionPath)" --SolutionDir "$(SolutionDir)\" --SolutionName "$(SolutionName)" --PlatformName "$(PlatformName)" --ProjectExt "$(ProjectExt)" --SolutionExt "$(SolutionExt)"') do set PARVAR=%%i 
prog\CustomNuGetBuilder\CustomNuGetBuilder.exe -c "conf\config.json" -p %PARVAR%
```


## How it works

![Structure Diagram](https://user-images.githubusercontent.com/40789489/90954771-c96fa600-e477-11ea-9ffc-778d26baf592.png)

## Context: Archived and now OSS
This is an old project which was created some time ago. <br/>
It was migrated to NET Core (should work now properly) and is now archived as OSS, so everyone can learn from it :book:
