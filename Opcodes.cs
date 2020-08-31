using System;
using System.Collections.Generic;
using System.Text;

namespace CCVM
{
    sealed partial class VM
    {
        // [opcode(1)]
        private void OpcodeExit()
        {
            flags[5] = true;
            
            PC++;
        }

        // [opcode(1) literal(4)] 5b
        private void OpcodePushLit()
        {
            stack.Push(Fetch32());
        }

        // [opcode(1) register(1)] 2b
        private void OpcodePushReg() // !
        {
            stack.Push(GetRegister(program[PC++]));
        }

        // [opcode(1) register(1)] 2b
        private void OpcodePopReg()
        {
            SetRegister(program[PC++], stack.Pop());
        }

        // [opcode(1) address(4)] 5b
        private void OpcodePopMemory()
        {
            SetMemory(Fetch32(), stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeDup()
        {
            stack.Push(stack.Peek());
        }

        // [opcode(1) register(1) literal(4)] 6b
        private void OpcodeMovLitToReg()
        {
            SetRegister(program[PC++], Fetch32());
        }

        // [opcode(1) address(4) literal(4)] 9b
        private void OpcodeMovLitToMem()
        {
            SetMemory(Fetch32(), Fetch32());
        }

        // [opcode(1) register(1) address(4)] 6b
        private void OpcodeMovAddressToReg()
        {
            SetRegister(program[PC++], GetMemory(Fetch32()));
        }

        // [opcode(1) address(4) register(1)] 6b
        private void OpcodeMovRegToAddress()
        {
            SetMemory(Fetch32(), GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeAddReg()
        {
            byte accumulatorID = program[PC++];
            UInt32 a = GetRegister(accumulatorID);
            UInt32 b = GetRegister(program[PC++]);
            SetRegister(accumulatorID, a + b);

            if (a > UInt32.MaxValue - b)
            {
                flags[4] = true;
            }
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeSubReg()
        {
            byte accumulatorID = program[PC++];
            UInt32 a = GetRegister(accumulatorID);
            UInt32 b = GetRegister(program[PC++]);
            SetRegister(accumulatorID, a - b);

            if (a < UInt32.MinValue + b)
            {
                flags[4] = true;
            }
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeDivReg()
        {
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) / GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeMulReg()
        {
            byte accumulatorID = program[PC++];
            UInt32 a = GetRegister(accumulatorID);
            UInt32 b = GetRegister(program[PC++]);
            SetRegister(accumulatorID, a * b);

            if (a > UInt32.MaxValue / (UInt64)b)
            {
                flags[4] = true;
            }
        }

        // [opcode(1) register(1)] 2b
        private void OpcodeNotReg()
        {
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, ~GetRegister(accumulatorID));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeAndReg()
        {
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) & GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeOrReg()
        {
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) | GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeXorReg()
        {
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) ^ GetRegister(program[PC++]));
        }

        // [opcode(1)] 1b
        private void OpcodeAddStack()
        {
            UInt32 a = stack.Pop();
            UInt32 b = stack.Pop();
            stack.Push(a + b);

            if (a > UInt32.MaxValue - b)
            {
                flags[4] = true;
            }
        }

        // [opcode(1)] 1b
        private void OpcodeSubStack()
        {
            UInt32 a = stack.Pop();
            UInt32 b = stack.Pop();
            stack.Push(a - b);

            if (a < UInt32.MinValue + b)
            {
                flags[4] = true;
            }
        }

        // [opcode(1)] 1b
        private void OpcodeDivStack()
        {
            stack.Push(stack.Pop() / stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeMulStack()
        {
            UInt32 a = stack.Pop();
            UInt32 b = stack.Pop();
            stack.Push(a * b);

            if (a > UInt32.MaxValue / (UInt64)b)
            {
                flags[4] = true;
            }
        }

        // [opcode(1)] 1b
        private void OpcodeNotStack()
        {
            stack.Push(~stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeAndStack()
        {
            stack.Push(stack.Pop() & stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeOrStack()
        {
            stack.Push(stack.Pop() | stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeXorStack()
        {
            stack.Push(stack.Pop() ^ stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeSyscall()
        {
            switch(GetRegister(0x00))
            {
                case 0x00:
                    { // cprint
                        UInt32 ptr = GetRegister(0x01);
                        UInt32 len = GetRegister(0x02);

                        for (int i = 0; i < len; i++)
                        {
                            Console.Write((char)memory[ptr + i]);
                        }
                        break;
                    }
                case 0x01:
                    { // cread
                        UInt32 ptr = GetRegister(0x01);

                        string input = Console.ReadLine();

                        for (int i = 0; i < input.Length; i++)
                        {
                            memory[ptr + i] = input[i];
                        }
                        break;
                    }
                case 0x02:
                    { // cclear
                        Console.Clear();
                        break;
                    }
            }
        }

        // [opcode(1) address(4)] 5b
        private void OpcodeJmpAbs()
        {
            
            Int32 Address = (Int32)Fetch32();
            if (Address < 0) // overflow happened
            {
                Environment.Exit(1);
            }

            PC = Address;
            PC += HeaderSize;
            PC--;
        }

        // [opcode(1) offset(4)] 5b
        private void OpcodeJmpRel()
        {
            Int32 Address = (Int32)Fetch32();
            PC += Address - 5;
        }

        // [opcode(1)] 1b
        private void OpcodeResetFlags()
        {
            for (UInt16 i = 0; i < flags.Length - 1; i++)
            {
                flags[i] = false;
            }
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeCompareRegisters()
        {
            UInt32 a = GetRegister(program[PC++]);
            UInt32 b = GetRegister(program[PC++]);

            if (a == b)
            {
                flags[0] = true;
            }
            
            if (a != b)
            {
                flags[1] = true;
            }
            
            if (a > b)
            {
                flags[2] = true;
            }
            
            if (a < b)
            {
                flags[3] = true;
            }
        }

        // [opcode(1) register(1) literal(4)] 6b
        private void OpcodeCompareRegistersLiteral()
        {
            UInt32 a = GetRegister(program[PC++]);
            UInt32 b = Fetch32();

            if (a == b)
            {
                flags[0] = true;
            }

            if (a != b)
            {
                flags[1] = true;
            }

            if (a > b)
            {
                flags[2] = true;
            }

            if (a < b)
            {
                flags[3] = true;
            }
        }

        // [opcode(1) literal(4)] 5b
        private void OpcodeCompareStackLitteral()
        {
            UInt32 a = stack.Peek();
            UInt32 b = Fetch32();

            if (a == b)
            {
                flags[0] = true;
            }

            if (a != b)
            {
                flags[1] = true;
            }

            if (a > b)
            {
                flags[2] = true;
            }

            if (a < b)
            {
                flags[3] = true;
            }
        }

        // [opcode(1) position(4)] 5b
        private void OpcodeJumpIfFlag_abs(byte flagID)
        {
            if (flags[flagID])
            {
                OpcodeJmpAbs();
            } 
            
            else
            {
                PC += 4;
            }
        }

        // [opcode(1) position(4)] 5b
        private void OpcodeJumpIfFlag_rel(byte flagID)
        {
            if (flags[flagID])
            {
                OpcodeJmpRel();
            }

            else
            {
                PC += 4;
            }
        }

        // [opcode(1) register(1)] 2b
        private void OpcodeIncrementRegister()
        {
            byte id = program[PC++];
            if (GetRegister(id) == uint.MaxValue)
            {
                flags[4] = true;
            }
            SetRegister(id,GetRegister(id)+1);
        }

        // [opcode(1) register(1)] 2b
        private void OpcodeDecrementRegister()
        {
            byte id = program[PC++];
            if (GetRegister(id) == 0)
            {
                flags[4] = true;
            }
            SetRegister(id, GetRegister(id) - 1);
        }

        // [opcode(1) address(4)] 2b
        private void OpcodeCall()
        {
            stack.Push(GetRegister(0));
            stack.Push(GetRegister(1));
            stack.Push(GetRegister(2));
            stack.Push(GetRegister(3));
            stack.Push((uint) PC);
            SBP = stack.Count;
            PC = (int)Fetch32();
            PC += HeaderSize;
        }

        // [opcode(1)] 1b
        private void OpcodeRet()
        {
            while (SBP != stack.Count)
            {
                stack.Pop();
            }

            PC = (int)stack.Pop();
            SetRegister(3, stack.Pop());
            SetRegister(2, stack.Pop());
            SetRegister(1, stack.Pop());
            SetRegister(0, stack.Pop());

            SBP = stack.Count;
        }
    }
}
