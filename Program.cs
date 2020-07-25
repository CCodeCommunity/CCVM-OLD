using System;
using System.Collections.Generic;

namespace CCVM
{
    class Program
    {
        static void Main(string[] args)    
        {
            // check if an executable was given
            if (args.Length <= 0)
            {
                Console.WriteLine("[ERROR]: No executable given");
                return;
            }

            // open the executable
            byte[] program = FileParser.Parse(args[0]);

            VM CCVM = new VM();

            CCVM.LoadProgram(program);
            CCVM.Initialize(1000);

            CCVM.Run();

            CCVM.PrintStack();
        }
    }
}
