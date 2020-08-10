using System;
using System.Collections;
using System.Collections.Generic;


namespace CCVM
{
    sealed partial class VM
    {
        private enum Registers
        {
            RegA, RegB, RegC, RegD
        }

        private byte[] program;
        private Stack<UInt32> stack = new Stack<UInt32>();
        private UInt32[] memory;

        private UInt32[] Regs = { 0, 0, 0, 0 };
        private Int32 PC = 0; // program counter

        private byte instruction;
        private BitArray flags = new BitArray( 6 );
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
                    OpcodeMovLitToReg();
                    break;
                case 0x07: 
                    OpcodeMovLitToMem();
                    break;
                case 0x08:
                    OpcodeMovAddressToReg();
                    break;
                case 0x09:
                    OpcodeMovRegToAddress();
                    break;
                case 0x10:
                    OpcodeAddReg();
                    break;
                case 0x11:
                    OpcodeAddStack();
                    break;
                case 0x12:
                    OpcodeSubReg();
                    break;
                case 0x13:
                    OpcodeSubStack();
                    break;
                case 0x14:
                    OpcodeMulReg();
                    break;
                case 0x15:
                    OpcodeMulStack();
                    break;
                case 0x16:
                    OpcodeDivReg();
                    break;
                case 0x17:
                    OpcodeDivStack();
                    break;
                case 0x18:
                    OpcodeNotReg();
                    break;
                case 0x19:
                    OpcodeNotStack();
                    break;
                case 0x1a:
                    OpcodeAndReg();
                    break;
                case 0x1b:
                    OpcodeAndStack();
                    break;
                case 0x1c:
                    OpcodeOrReg();
                    break;
                case 0x1d:
                    OpcodeOrStack();
                    break;
                case 0x1e:
                    OpcodeXorReg();
                    break;
                case 0x1f:
                    OpcodeXorStack();
                    break;
                case 0x20:
                    OpcodeJmpAbs();
                    break;
                case 0x21:
                    OpcodeJmpRel();
                    break;
                case 0x30:
                    OpcodeCompareRegisters();
                    break;
                case 0x31:
                    OpcodeCompareRegistersLiteral();
                    break;
                case 0x32:
                    OpcodeCompareStackLitteral();
                    break;
                case 0x40:
                    OpcodeResetFlags();
                    break;
                case 0xff:
                    OpcodeSyscall();
                    break;
            }
        }

        public void PrintStack()
        {
            Console.WriteLine("stack:");
            if (stack.Count == 0)
            {
                Console.WriteLine("   *empty*");
            }

            else
            {
                foreach (UInt32 b in stack)
                {
                    Console.WriteLine($"   {b}");
                }
            }
        }

        public void PrintRegs()
        {
            Console.WriteLine("registers:");
            Console.WriteLine($"   A: {Regs[(byte)Registers.RegA]}");
            Console.WriteLine($"   B: {Regs[(byte)Registers.RegB]}");
            Console.WriteLine($"   C: {Regs[(byte)Registers.RegC]}");
            Console.WriteLine($"   D: {Regs[(byte)Registers.RegD]}");
        }

        public void PrintMem()
        {
            Console.WriteLine("Memory:");
            string PerapsEnter;
            for (int i = 0; i < 128; i++)
            {
                if (i % 16 == 0)
                {
                    PerapsEnter = "\n";
                    if (i == 0)
                    {
                        PerapsEnter = "";
                    }
                    Console.Write($"{PerapsEnter}[0x{(i).ToString("X4")}]   ");
                }

                Console.Write($"{"0x" + memory[i].ToString("X2")} ");
            }
        }

        public void PrintFlags()
        {
            string[] flagNames = { "equal:\t", "not equal:\t", "greater:\t", "smaller:\t", "overflow:\t", "stop:\t" };
            Console.WriteLine("\nflags: ");
            byte i = 0;
            foreach (bool flag in flags)
            {
                Console.WriteLine($"   {flagNames[i++]}{flag}");
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
            while(!flags[5])
            {
                Fetch();
                Execute();
            }
        }
    }
}
