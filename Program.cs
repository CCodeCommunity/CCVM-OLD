using System;
using System.Collections.Generic;
using CCVM.CCAssembler;

namespace CCVM
{
    class Program
    {
        static void Main(string[] args)    
        {
            // check if an executable was given
            if (args.Length <= 0)
            {
                Console.WriteLine("[ERROR]: No file given");
                return;
            }

            

            // run the cc binary
            if (args[0].EndsWith(".ccb") || args[0].EndsWith(".CCB"))
            {
                byte[] content = FileParser.ParseBytes(args[0]);
                VM CCVM = new VM();

                CCVM.LoadProgram(content);
                CCVM.Initialize(1000);

                CCVM.Run();

                Console.WriteLine("");
                CCVM.PrintStack();

                Console.WriteLine("");
                CCVM.PrintRegs();

                Console.WriteLine("");
                CCVM.PrintMem();
            }
            
            // assemble the cc assembly
            else if (args[0].EndsWith(".cca") || args[0].EndsWith(".CCA"))
            {
                Console.WriteLine("Assembling...");

                string content = FileParser.ParseString(args[0]);
                
                Assembler assembler = new Assembler();
                assembler.LoadAssembly(content);
                assembler.Lex();
                assembler.GenerateCode(args[0].Split(".")[0] + ".ccb");
                Console.WriteLine("Done!");
            }

            // error
            else
            {
                Console.WriteLine($"[ERROR] CCVM does not know what to do with file extension \".{args[0].Split(".")[1]}\"");
            }

        }
    }
}
