using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CCVM
{
    class FileParser
    {
        public static byte[] Parse(string FileName)
        {
            byte[] program = File.ReadAllBytes(FileName);
            return program;
        }
    }
}
