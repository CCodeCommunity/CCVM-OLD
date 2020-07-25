using System;
using System.Collections.Generic;


namespace CCVM
{
    class VM
    {
        private byte[] program;
        private Stack<UInt32> stack = new Stack<UInt32>();
        private UInt32[] memory;

        private byte exit = 0;

        private Int32 PC = 0; // program counter
        private Int32 SP = 0; // stack pointer

        private byte instruction;

        private UInt32 Fetch32()
        {
            UInt32 V = 0;
            byte[] bytes = new byte[4];
            Array.Copy(program, PC, bytes, 0, 4);
            PC += 4;
            foreach (byte b in bytes)
            {
                V <<= 8;
                V += b;
            }

            return V;
        }

        private void Fetch()
        {
            instruction = program[PC++]; // fetch the instruction
        }

        private void Execute()
        {
            switch (instruction)
            {
                case 0x00:
                    {
                        Console.WriteLine("Exit");
                        exit = 1;
                        PC++;
                        break;
                    }
                case 0x01:
                    {
                        Console.WriteLine("Pusing literal to stack");
                        stack.Push(Fetch32());
                        break;
                    }
            }
        }

        public void PrintStack()
        {
            Console.WriteLine("stack: ");

            foreach(UInt32 b in stack)
            {
                Console.WriteLine(b);
            } 
        }

        public void LoadProgram(byte[] program)
        {
            this.program = program;
        }

        public void Initialize(UInt32 memSize)
        {
            memory = new UInt32[memSize];
        }

        public void Run()
        {
            while(exit == 0)
            {
                Fetch();
                Execute();
            }
        }
    }
}
