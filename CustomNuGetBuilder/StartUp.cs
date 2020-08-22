using CoreFramework.Config;
using CustomNuGetBuilder.Building;
using CustomNuGetBuilder.Building.Config;
using CustomNuGetBuilder.CmdOptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace CustomNuGetBuilder
{
   public class StartUp
   {
      private CmdOption CmdOption { get; set; }

      private BuildConfig Config { get; set; } = new BuildConfig();

      public StartUp(CmdOption cmdOption)
      {
         CmdOption = cmdOption;
      }

      public void Start()
      {
         Contract.Requires(CmdOption != null);
         if (!string.IsNullOrWhiteSpace(CmdOption.WorkDir) && Directory.Exists(CmdOption.WorkDir))
         {
            Log.Info($"Setting current workdir to '{CmdOption.WorkDir}'");
            Directory.SetCurrentDirectory(CmdOption.WorkDir);
         }
         Log.Info($"Current directory is '{Directory.GetCurrentDirectory()}'");

         if (CmdOption.ConfigGenerationPath != null)
         {
            Log.Info("MODE: Write JSON Config");

            WriteConfig();
            return;
         }

         Log.Info("MODE: Normal start");
         if (CmdOption.ConfigPath != null)
            ReadConfig();

         DoStart();
      }

      protected void WriteConfig()
      {
         Log.Info("Writing json config");

         if (!string.IsNullOrWhiteSpace(CmdOption.ConfigGenerationPath))
            Config.Config.SavePath = CmdOption.ConfigGenerationPath;

         Log.Info($"Saving '{Config.Config.SavePath}'");
         Config.Save();

         Log.Info($"Saving: success");
      }

      protected void ReadConfig()
      {
         Log.Info("Reading json config");

         if (!string.IsNullOrWhiteSpace(CmdOption.ConfigPath))
            Config.Config.SavePath = CmdOption.ConfigPath;

         Log.Info($"Loading '{Config.Config.SavePath}'");
         Config.Load(LoadFileNotFoundAction.THROW_EX);

         Log.Info($"Loading: success");
      }


      protected void DoStart()
      {
         Log.Info("Starting");
         new Build(CmdOption, Config).Execute();
         Log.Info("Done");
      }
   }
}
