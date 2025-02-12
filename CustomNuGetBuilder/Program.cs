﻿using CommandLine;
using CustomNuGetBuilder.Building;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomNuGetBuilder.CmdOptions;
using CustomNuGetBuilder.Building.Config;
using System.Reflection;
using Serilog;

namespace CustomNuGetBuilder
{
   public static class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine($"****** {Assembly.GetEntryAssembly().GetName().Name} {Assembly.GetEntryAssembly().GetName().Version} ******");

         var logConf =
            new LoggerConfiguration()
            .Enrich.WithThreadId()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss,fff} {Level:u3} {ThreadId,-2} {Message:lj}{NewLine}{Exception}");

         Serilog.Log.Logger = logConf.CreateLogger();

         AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
         {
            Log.Info("Shutting down logger; Flushing...");
            Serilog.Log.CloseAndFlush();
         };

#if !DEBUG
         try
         {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
         Parser.Default.ParseArguments<CmdOption>(args)
                     .WithParsed((opt) =>
                     {
                        var starter = new StartUp(opt);
                        starter.Start();
                     })
                     .WithNotParsed((ex) =>
                     {
                        if (ex.All(err =>
                                new ErrorType[]
                                {
                                 ErrorType.HelpRequestedError,
                                 ErrorType.HelpVerbRequestedError
                                }.Contains(err.Tag))
                          )
                           return;

                        foreach (var error in ex)
                           Log.Error($"Failed to parse: {error.Tag}");

                        Log.Fatal("Failure processing args");
                     });
#if !DEBUG
         }
         catch (Exception ex)
         {
            Log.Fatal(ex);
         }
#endif
      }

      private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs ev)
      {
         try
         {
            Log.Fatal("Detected UnhandledException");
            if (ev.ExceptionObject is Exception ex)
               Log.Fatal("Run into unhandled error", ex);
            else
               Log.Fatal($"Run into unhandled error: {ev.ExceptionObject}");
         }
         catch (Exception ex)
         {
            Console.Error.WriteLine(ex);
         }
      }
   }
}
