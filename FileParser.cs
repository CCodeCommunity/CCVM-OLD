using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CCVM
{
    class FileParser
    {
        public static byte[] ParseBytes(string FileName)
        {
            return File.ReadAllBytes(FileName);
        }

        public static string ParseString(string FileName)
        {
            return File.ReadAllText(FileName);
        }
    }
}
