using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CCVM
{
    class ArgParser
    {
        private static List<string> options = new List<string>();
        public static void Parse(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    options.Add(arg);
                }
            }
        }

        public static bool Option(string o)
        {
            return options.Contains(o);
        }
    }
}
