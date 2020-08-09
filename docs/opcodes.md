# CCVM developer docs (binary format)

## byte types

The bytes in a CCVM executable (.ccb file) can represent multiple things:

- An opcode:

  An opcode represents a instruction. Since there are a total of 256 possibilities that a single byte can have, CCVM has a grand total of 256 instructions/opcodes. for example: the opcode `0x00` means to stop the program.

  

- A literal:

  A CCVM literal is an unsigned 32 bit number. Since its 32 bits instead of 8 bit, it will take up 4 bytes in the executable. for example, the literal `0x00 0x00 0x00 0x0f` stands for the hex number `0xF` which means 15 in decimal.

  

- A register ID:

  A register ID is a singly byte in the binary that represents the identification number of a register in the machine, the CCVM has 4 registers (a, b, c and d). Which are indexed in the following way:

  | reregister name | bytecode |
  | --------------- | -------- |
  | Register A      | 0x00     |
  | Register b      | 0x01     |
  | Register c      | 0x02     |
  | Register d      | 0x03     |

  If an ID is given that is higher then 0x03, the VM will crash.

  

- An address:

  An Address is a 4 byte number that points to somewhere in the VM memory, or the program data. It can be used to store data in RAM memory, or to jump to an absolute position in the program.

  

- An offset:

  An offset is similar to an Address, but its signed and only used to jump in the program. Opposed to an address, an offset is for jumping to a **relative** position in the program data, instead of **absolute**.

  

## CCVM instruction set

LIT = literal
REG = register ID
ADD = address
OFF = offset

| Name    | Opcode | Description                          | Arguments |
| ------- | ------ | ------------------------------------ | --------- |
| STOP    | 0x00   | Exit the program and quit the VM     | -         |
| PSH     | 0x01   | Push a literal to the stack          | LIT       |
| PSH     | 0x02   | Push a register to the stack         | REG       |
| POP     | 0x03   | Pop the stack to a register          | REG       |
| POP     | 0x04   | Pop the stack to a memory address    | ADD       |
| DUP     | 0x05   | Duplicate the top item on the stack  | -         |
| MOV     | 0x06   | move a literal to a register         | REG, LIT  |
| MOV     | 0x07   | Move a literal to a memory address   | ADD, LIT  |
| MOV     | 0x08   | Move a memory address to a register  | REG, ADD  |
| MOV     | 0x09   | Move a register to a memory address  | ADD, REG  |
| ADD     | 0x10   | Add 2 registers together             | REG, REG  |
| ADD     | 0x11   | Add top 2 stack items together       | -         |
| SUB     | 0x12   | Subtract 2 registers                 | REG, REG  |
| SUB     | 0x13   | Subtract top 2 stack items together  | -         |
| MUL     | 0x14   | Multiply 2 registers                 | REG, REG  |
| MUL     | 0x15   | Multiply top 2 stack items together  | -         |
| DIV     | 0x16   | Divide 2 registers                   | REG, REG  |
| DIV     | 0x17   | Divide top 2 stack items             | -         |
| NOT     | 0x18   | Logical NOT on register              | REG       |
| NOT     | 0x19   | Logical NOT on top stack item        | -         |
| AND     | 0x1A   | Logical AND on 2 registers           | REG, REG  |
| AND     | 0x1B   | Logical AND on top 2 stack items     | -         |
| OR      | 0x1C   | Logical OR on 2 registers            | REG, REG  |
| OR      | 0x1D   | Logical OR on top 2 stack items      | -         |
| XOR     | 0x1E   | Logical XOR on 2 registers           | REG, REG  |
| XOR     | 0x1F   | Logical XOR on top 2 stack items     | -         |
| JMP     | 0x20   | Jump to absolute position in program | ADD       |
| JMP     | 0x21   | Jump to relative position in program | OFF       |
| FRS     | 0x40   | Reset all flags                      | -         |
| SYSCALL | 0xFF   | Perform a syscall                    | -         |

