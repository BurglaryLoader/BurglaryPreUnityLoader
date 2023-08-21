using Burglary.cons;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using ct = Burglary.cons.ConsoleUtils;

namespace BurglaryPreUnityLoader
{
    internal class IsNETOperation
    {
        protected bool isNET = false;
        protected Assembly output = null;
        public IsNETOperation(string filepath)
        {
            isNET = false;
            output = null;
            try
            {
                output = AppDomain.CurrentDomain.Load(filepath); // since passing the filepath doesnt work ig
                isNET = true;
            }
            catch (Exception ex)
            {
                var writer = Entrypoint.writer;
                ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                ct.WLColNL("ERROR! (asm load)", ConsoleColor.DarkRed, writer);
                ct.WLColNL("message: " + ex.Message, ConsoleColor.Red, writer);
                ct.WLColNL("stacktrace: " + ex.StackTrace, ConsoleColor.Red, writer);
                ct.WLColNL("datadict: " + ex.Data.ToString(), ConsoleColor.Red, writer);
                ct.WLColNL("targetsite: " + ex.TargetSite.Name, ConsoleColor.Red, writer);
                ct.WLColNL("---------------------", ConsoleColor.DarkRed, writer);
                ct.WLColNL("RAW: " + ex.ToString(), ConsoleColor.Red, writer);
                ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
            }
        }

        public bool GetResult()
        {
            return isNET;
        }
        public Assembly GetAssembly()
        {
            return output;
        }
    }
}
