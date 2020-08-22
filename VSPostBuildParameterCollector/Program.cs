using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;

namespace VSPostBuildParameterCollector
{
   public static class Program
   {
      static void Main(string[] args)
      {
         Parser.Default.ParseArguments<VSParsOption>(args)
           .WithParsed(opts =>
           {
              var plaintext = JsonConvert.SerializeObject(opts);

              var plainTextBytes = Encoding.UTF8.GetBytes(plaintext);

              Console.WriteLine(Convert.ToBase64String(plainTextBytes));
           })
           .WithNotParsed(errs =>
           {
              Console.WriteLine("ERROR");
           });
      }
   }
}
