using System;
using System.Collections.Generic;
using System.Text;

namespace CCVM
{
    class VM
    {
        private List<byte> program;
        private Stack<UInt32> stack = new Stack<UInt32>();
        private UInt32[] memory;

        private byte exit = 0;

        private UInt32 PC = 0; // program counter
        private UInt32 SP = 0; // stack pointer

        private byte instruction;

        private UInt32 Fetch32()
        {
            UInt32 V = 0;

            for (int i = 0; i < 4; i++)
            {
                V += program[(int)PC++];
                if (i != 3)
                    V <<= 8;
            }

            return V;
        }

        private void Fetch()
        {
            instruction = program[(int)PC++]; // fetch the instruction
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
                        UInt32 Val = Fetch32();
                        stack.Push(Val);
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

        public void LoadProgram(List<byte> program)
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
