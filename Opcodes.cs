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
            exit = 1;
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
            SetRegister(accumulatorID, GetRegister(accumulatorID) + GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeSubReg()
        {
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) - GetRegister(program[PC++]));
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
            SetRegister(accumulatorID, GetRegister(accumulatorID) * GetRegister(program[PC++]));
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
            stack.Push(stack.Pop() + stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeSubStack()
        {
            UInt32 first = stack.Pop();
            stack.Push(stack.Pop() - first);
        }

        // [opcode(1)] 1b
        private void OpcodeDivStack()
        {
            UInt32 first = stack.Pop();
            stack.Push(stack.Pop() / first);
        }

        // [opcode(1)] 1b
        private void OpcodeMulStack()
        {
            stack.Push(stack.Pop() * stack.Pop());
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
                case 0: // print
                    UInt32 ptr = GetRegister(0x01);
                    UInt32 len = GetRegister(0x02);

                    for (int i = 0; i < len; i++)
                    {
                        Console.Write((char) memory[ptr+i]);
                    }
                    break;
            }
        }
    }
}
