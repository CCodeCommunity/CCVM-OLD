using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CCVM
{
    class FileParser
    {
        public static List<byte> Parse(string FileName)
        {
            List<byte> program = new List<byte>();
            string content = File.ReadAllText(FileName);

            foreach (char c in content)
            {
                program.Add((byte)c);
            }

            return program;
        }
    }
}
