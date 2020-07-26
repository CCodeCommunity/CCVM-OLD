using System;
using System.Collections.Generic;


namespace CCVM
{
    sealed class VM
    {
        private enum Registers
        { 
            RegA, RegB, RegC, RegD
        }

        private byte[] program;
        private Stack<UInt32> stack = new Stack<UInt32>();
        private UInt32[] memory;

        private UInt32[] Regs = {0, 0, 0, 0}; 

        private byte exit = 0;

        private Int32 PC = 0; // program counter

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

        private void SetRegister(byte ID, UInt32 Value)
        {
            Regs[ID] = Value;
        }

        private UInt32 GetRegister(byte ID)
        {
            return Regs[ID];
        }

        private void SetMemory(UInt32 Address, UInt32 Value)
        {
            memory[Address] = Value;
        }

        private UInt32 GetMemory(UInt32 Address)
        {
            return memory[Address];
        }

        private void Fetch()
        {
            instruction = program[PC++]; // fetch the instruction
        }

        // [opcode(1)]
        private void OpcodeExit()
        {
            Console.WriteLine("Exit");
            exit = 1;
            PC++;
        }

        // [opcode(1) literal(4)] 5b
        private void OpcodePushLit()
        {
            Console.WriteLine("Pusing literal to stack");
            stack.Push(Fetch32());
        }

        // [opcode(1) literal(4)] 5b
        private void OpcodePushReg() // !
        {
            Console.WriteLine("Pusing register to stack");
            stack.Push(GetRegister(program[PC++]));
        }

        // [opcode(1) register(1)] 2b
        private void OpcodePopReg()
        {
            Console.WriteLine("popping to register");
            SetRegister(program[PC++], stack.Pop());
        }

        // [opcode(1) address(4)] 5b
        private void OpcodePopMemory()
        {
            Console.WriteLine("popping to memory");
            SetMemory(Fetch32(), stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeDup()
        {
            Console.WriteLine("duping stack");
            stack.Push(stack.Peek());
        }

        // [opcode(1) register(1) literal(4)] 6b
        private void OpcodeMovLitReg()
        {
            Console.WriteLine("Moving literal to register");
            SetRegister(program[PC++], Fetch32());
        }

        // [opcode(1) address(4) literal(4)] 9b
        private void OpcodeMovRegLit()
        {
            Console.WriteLine("Moving literal to memory");
            SetMemory(Fetch32(), Fetch32());
        }

        // [opcode(1) register(1) address(4)] 6b
        private void OpcodeMovAdrressReg()
        {
            Console.WriteLine("Moving address to register");
            SetRegister(program[PC++], GetMemory(Fetch32()));
        }

        // [opcode(1) address(4) register(1)] 6b
        private void OpcodeMovRegAdrress()
        {
            Console.WriteLine("Moving register to address");
            SetMemory(Fetch32(), GetRegister(program[PC++]));
        }

        private void Execute()
        {
            switch (instruction)
            {
                case 0x00: 
                    OpcodeExit();
                    break;
                case 0x01: 
                    OpcodePushLit();
                    break;
                case 0x02:
                    OpcodePushReg();
                    break;
                case 0x03: 
                    OpcodePopReg();
                    break;
                case 0x04: 
                    OpcodePopMemory();
                    break;
                case 0x05: 
                    OpcodeDup();
                    break;
                case 0x06: 
                    OpcodeMovLitReg();
                    break;
                case 0x07: 
                    OpcodeMovRegLit();
                    break;
                case 0x08: 
                    OpcodeMovAdrressReg();
                    break;
                case 0x09: 
                    OpcodeMovRegAdrress();
                    break;
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

        public void PrintRegs()
        {
            Console.WriteLine($"A: {Regs[(byte)Registers.RegA]}");
            Console.WriteLine($"B: {Regs[(byte)Registers.RegB]}");
            Console.WriteLine($"C: {Regs[(byte)Registers.RegC]}");
            Console.WriteLine($"D: {Regs[(byte)Registers.RegD]}");
        }

        public void PrintMem()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.Write($"{memory[i]} ");
            }
        }

        public void LoadProgram(byte[] program)
        {
            this.program = program;
        }

        public void Initialize(UInt32 memSize)
        {
            memory = new UInt32[memSize];
            Array.Fill<UInt32>(memory, 0x00);
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
