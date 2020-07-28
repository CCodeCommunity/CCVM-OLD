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

        // [opcode(1) register(1)] 2b
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
        private void OpcodeMovLitToReg()
        {
            Console.WriteLine("Moving literal to register");
            SetRegister(program[PC++], Fetch32());
        }

        // [opcode(1) address(4) literal(4)] 9b
        private void OpcodeMovLitToMem()
        {
            Console.WriteLine("Moving literal to memory");
            SetMemory(Fetch32(), Fetch32());
        }

        // [opcode(1) register(1) address(4)] 6b
        private void OpcodeMovAddressToReg()
        {
            Console.WriteLine("Moving address to register");
            SetRegister(program[PC++], GetMemory(Fetch32()));
        }

        // [opcode(1) address(4) register(1)] 6b
        private void OpcodeMovRegToAddress()
        {
            Console.WriteLine("Moving register to address");
            SetMemory(Fetch32(), GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeAddReg()
        {
            Console.WriteLine("adding registers");
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) + GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeSubReg()
        {
            Console.WriteLine("subtracting registers");
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) - GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeDivReg()
        {
            Console.WriteLine("dividing registers");
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) / GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeMulReg()
        {
            Console.WriteLine("multiplying registers");
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) * GetRegister(program[PC++]));
        }

        // [opcode(1) register(1)] 2b
        private void OpcodeNotReg()
        {
            Console.WriteLine("NOTing register");
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, ~GetRegister(accumulatorID));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeAndReg()
        {
            Console.WriteLine("ANDing registers");
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) & GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeOrReg()
        {
            Console.WriteLine("ORing registers");
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) | GetRegister(program[PC++]));
        }

        // [opcode(1) register(1) register(1)] 3b
        private void OpcodeXorReg()
        {
            Console.WriteLine("XORing registers");
            byte accumulatorID = program[PC++];
            SetRegister(accumulatorID, GetRegister(accumulatorID) ^ GetRegister(program[PC++]));
        }

        // [opcode(1)] 1b
        private void OpcodeAddStack()
        {
            Console.WriteLine("adding stack");
            stack.Push(stack.Pop() + stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeSubStack()
        {
            Console.WriteLine("subtracting stack");
            UInt32 first = stack.Pop();
            stack.Push(stack.Pop() - first);
        }

        // [opcode(1)] 1b
        private void OpcodeDivStack()
        {
            Console.WriteLine("dividing stack");
            UInt32 first = stack.Pop();
            stack.Push(stack.Pop() / first);
        }

        // [opcode(1)] 1b
        private void OpcodeMulStack()
        {
            Console.WriteLine("multiplying stack");
            stack.Push(stack.Pop() * stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeNotStack()
        {
            Console.WriteLine("NOTing stack");
            stack.Push(~stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeAndStack()
        {
            Console.WriteLine("ANDing stack");
            stack.Push(stack.Pop() & stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeOrStack()
        {
            Console.WriteLine("ORing stack");
            stack.Push(stack.Pop() | stack.Pop());
        }

        // [opcode(1)] 1b
        private void OpcodeXorStack()
        {
            Console.WriteLine("XORing stack");
            stack.Push(stack.Pop() ^ stack.Pop());
        }
    }
}
