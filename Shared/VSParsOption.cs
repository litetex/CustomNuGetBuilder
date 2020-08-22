using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
   public class VSParsOption
   {
      //Call:
      //VSPostBuildParameterCollector.exe --OutDir $(OutDir) --ConfigurationName $(ConfigurationName) --ProjectName $(ProjectName) --TargetName $(TargetName) --TargetPath $(TargetPath) --ProjectPath $(ProjectPath) --ProjectFileName $(ProjectFileName) --TargetExt $(TargetExt) --TargetFileName $(TargetFileName) --DevEnvDir $(DevEnvDir) --TargetDir $(TargetDir) --ProjectDir $(ProjectDir) --SolutionFileName $(SolutionFileName) --SolutionPath $(SolutionPath) --SolutionDir $(SolutionDir) --SolutionName $(SolutionName) --PlatformName $(PlatformName) --ProjectExt $(ProjectExt) --SolutionExt $(SolutionExt)

      // "abc\def\" --> args: args\def"  --> \\" == \"
      // \" --> " in args ...

      [Option("OutDir", Required = true)]
      public string OutDir { get; set; }


      [Option("ConfigurationName", Required = true)]
      public string ConfigurationName { get; set; }


      [Option("ProjectName", Required = true)]
      public string ProjectName { get; set; }


      [Option("TargetName", Required = true)]
      public string TargetName { get; set; }


      [Option("TargetPath", Required = true)]
      public string TargetPath { get; set; }


      [Option("ProjectPath", Required = true)]
      public string ProjectPath { get; set; }


      [Option("ProjectFileName", Required = true)]
      public string ProjectFileName { get; set; }


      [Option("TargetExt", Required = true)]
      public string TargetExt { get; set; }


      [Option("TargetFileName", Required = true)]
      public string TargetFileName { get; set; }


      [Option("DevEnvDir", Required = true)]
      public string DevEnvDir { get; set; }


      [Option("TargetDir", Required = true)]
      public string TargetDir { get; set; }


      [Option("ProjectDir", Required = true)]
      public string ProjectDir { get; set; }


      [Option("SolutionFileName", Required = true)]
      public string SolutionFileName { get; set; }


      [Option("SolutionPath", Required = true)]
      public string SolutionPath { get; set; }


      [Option("SolutionDir", Required = true)]
      public string SolutionDir { get; set; }


      [Option("SolutionName", Required = true)]
      public string SolutionName { get; set; }


      [Option("PlatformName", Required = true)]
      public string PlatformName { get; set; }


      [Option("ProjectExt", Required = true)]
      public string ProjectExt { get; set; }


      [Option("SolutionExt", Required = true)]
      public string SolutionExt { get; set; }
   }
}
