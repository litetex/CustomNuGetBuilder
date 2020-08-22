using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNuGetBuilder.CmdOptions
{
   public class CmdOption
   {
      //Call:
      //CustomNuGetBuilder.exe -c D:\...\config.json 

      [Option('c', "config", Required = true, HelpText = "ConfigPath")]
      public string ConfigPath { get; set; }

      [Option('p',"par", Required = true, HelpText = "VSParas as Json object encoded in base64")]
      public string Base64JsonPars { get; set; }

      [Option("genconf", Required = true, HelpText = "Generates default config in mentioned path")]
      public string ConfigGenerationPath { get; set; } = null;


      [Option('w',"workdir", HelpText = "Working directory")]
      public string WorkDir { get; set; } = null;
   }
}
