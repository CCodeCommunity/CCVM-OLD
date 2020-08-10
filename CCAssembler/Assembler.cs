using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CCVM.CCAssembler
{
    sealed class Assembler
    {
        private enum TokenType
        {
            Opcode,
            Literal,
            Label,
            Register,
            Address,
            Comma,
            Undefined
        }
        
        private class Token
        {
            public string Value = "";
            public TokenType Type = TokenType.Undefined;
            public int LineFound = 0;

            public void Print()
            {
                Console.WriteLine($"Token[type={Type} value={Value}]");
            }

            public Token Clone()
            {
                Token ret = new Token();
                ret.Value = Value;
                ret.Type = Type;
                ret.LineFound = LineFound;
                return ret;
            }

            public void Reset()
            {
                Value = "";
                Type = TokenType.Undefined;
                LineFound = 0;
            }
        }

        private string Assembly;
        private List<Token> Tokens = new List<Token>();
        private bool CommentMode = false;
        public void LoadAssembly(string assembly)
        {
            Assembly = assembly.Replace("\t", "").Replace("\r","").Replace("\n\n", "\n");
        }

        public void Lex()
        {
            Token CurrTok = new Token();
            int ReadingPos = -1;
            int LineCount = 1;
            while (ReadingPos++ < Assembly.Length-1)
            {
                char CurrChar = Assembly[ReadingPos];

                if (CurrChar == '\n')
                    LineCount++;

                if (CurrChar == 13)
                    continue;

                if (CommentMode)
                {
                    CommentMode = CurrChar == '\n' ? false : true;
                    continue;
                }

                if (CurrChar == ';')
                {
                    CommentMode = true;
                    if (CurrTok.Value != "")
                    {
                        if (CurrTok.Type == TokenType.Opcode && (CurrTok.Value == "a" || CurrTok.Value == "b" || CurrTok.Value == "c" || CurrTok.Value == "d"))
                        {
                            CurrTok.Type = TokenType.Register;
                        }
                        CurrTok.LineFound = LineCount;
                        Tokens.Add(CurrTok.Clone());
                        CurrTok.Reset();
                    }
                }

                else if ((CurrChar == ' ' || CurrChar == '\n') && CurrTok.Value != "")
                {
                    if (CurrTok.Type == TokenType.Opcode && (CurrTok.Value == "a" || CurrTok.Value == "b" || CurrTok.Value == "c" || CurrTok.Value == "d"))
                    {
                        CurrTok.Type = TokenType.Register;
                    }
                    CurrTok.LineFound = LineCount;
                    Tokens.Add(CurrTok.Clone());
                    CurrTok.Reset();
                    continue;
                }

                else if (CurrChar == ' ')
                    continue;

                else if (Char.IsLetter(CurrChar) && (CurrTok.Type == TokenType.Undefined || CurrTok.Type == TokenType.Opcode))
                {
                    CurrTok.Type = TokenType.Opcode;
                    CurrTok.Value += CurrChar;
                }

                else if (CurrChar == '-')
                {
                    CurrTok.Type = TokenType.Literal;
                    CurrTok.Value += CurrChar;
                }

                else if (Char.IsDigit(CurrChar) || CurrTok.Type == TokenType.Literal)
                {
                    if (CurrTok.Type != TokenType.Address)
                        CurrTok.Type = TokenType.Literal;
                    CurrTok.Value += CurrChar;
                }

                else if (CurrChar == ',')
                {
                    if (CurrTok.Value != "")
                    {
                        if (CurrTok.Type == TokenType.Opcode && (CurrTok.Value == "a" || CurrTok.Value == "b" || CurrTok.Value == "c" || CurrTok.Value == "d"))
                        {
                            CurrTok.Type = TokenType.Register;
                        }
                        CurrTok.LineFound = LineCount;
                        Tokens.Add(CurrTok.Clone());
                        CurrTok.Reset();
                    }
                    CurrTok.Type = TokenType.Comma;
                    CurrTok.Value = ",";
                    CurrTok.LineFound = LineCount;
                    Tokens.Add(CurrTok.Clone());
                    CurrTok.Reset();
                }

                else if (CurrChar == '&')
                {
                    CurrTok.Type = TokenType.Address;
                }

                else
                {
                    Console.WriteLine($"[ERROR] unexpected symbol on line {LineCount}: {CurrChar}");
                    Environment.Exit(1);
                }
            }

            if (CurrTok.Value != "")
            {
                if (CurrTok.Type == TokenType.Opcode && (CurrTok.Value == "a" || CurrTok.Value == "b" || CurrTok.Value == "c" || CurrTok.Value == "d"))
                {
                    CurrTok.Type = TokenType.Register;
                }
                CurrTok.LineFound = LineCount;
                Tokens.Add(CurrTok.Clone());
                CurrTok.Reset();
            }
        }

        public void PrintTokens()
        {
            foreach (Token T in Tokens)
            {
                T.Print();
            }
        }

        private void TokenAssert(TokenType expected, Token Got)
        {
            if (Got.Type != expected)
            {
                Console.WriteLine($"[ERROR] lexical error on line {Got.LineFound}: expected {expected}, but got {Got.Type}");
                Environment.Exit(1);
            }

        }

        private byte IdOfRegister(string registerName)
        {
            return (byte)(((byte)registerName[0]) - 97);
        }

        public void GenerateCode(string FileName)
        {
            List<byte> bytecode = new List<byte>();

            int TC = 0; // token counter

            while(TC < Tokens.Count)
            {
                Token T = Tokens[TC++];
                TokenAssert(TokenType.Opcode, T);
                switch (T.Value)
                {
                    case "stp":
                        bytecode.Add(0x00);
                        break;
                    case "psh":
                        if (Tokens[TC].Type == TokenType.Literal)
                        {
                            byte[] number = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(number);
                            bytecode.Add(0x01);
                            bytecode.AddRange(number);
                        }

                        else if (Tokens[TC].Type == TokenType.Register)
                        {
                            bytecode.Add(0x02);
                            bytecode.Add(IdOfRegister(Tokens[TC].Value));
                        }
                        TC += 1;
                        break;
                    case "pop":
                        if (Tokens[TC].Type == TokenType.Register)
                        {
                            bytecode.Add(0x03);
                            bytecode.Add(IdOfRegister(Tokens[TC].Value));
                        }

                        else if (Tokens[TC].Type == TokenType.Address)
                        {
                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.Add(0x04);
                            bytecode.AddRange(address);
                        }
                        TC += 1;
                        break;
                    case "dup":
                        bytecode.Add(0x05);
                        break;
                    case "mov":
                        TokenAssert(TokenType.Comma, Tokens[TC+1]);
                        if (Tokens[TC].Type == TokenType.Register && Tokens[TC+2].Type == TokenType.Literal)
                        {
                            bytecode.Add(0x06);
                            bytecode.Add(IdOfRegister(Tokens[TC].Value));

                            byte[] number = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC+2].Value));
                            Array.Reverse(number);
                            bytecode.AddRange(number);
                        }

                        else if (Tokens[TC].Type == TokenType.Address && Tokens[TC + 2].Type == TokenType.Literal)
                        {
                            bytecode.Add(0x07);

                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);

                            byte[] number = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC + 2].Value));
                            Array.Reverse(number);
                            bytecode.AddRange(number);
                        }

                        else if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Address)
                        {
                            bytecode.Add(0x08);

                            bytecode.Add(IdOfRegister(Tokens[TC].Value));

                            byte[] number = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC + 2].Value));
                            Array.Reverse(number);
                            bytecode.AddRange(number);
                        }

                        else if (Tokens[TC].Type == TokenType.Address && Tokens[TC + 2].Type == TokenType.Register)
                        {
                            bytecode.Add(0x09);

                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);

                            bytecode.Add(IdOfRegister(Tokens[TC + 2].Value));
                        }
                        TC += 3;
                        break;
                    case "add":
                        if (Tokens[TC].Type == TokenType.Opcode)
                        {
                            bytecode.Add(0x11);
                            
                        }

                        else
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC+1]);
                            if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Register)
                            {
                                bytecode.Add(0x10);

                                bytecode.Add(IdOfRegister(Tokens[TC].Value));
                                bytecode.Add(IdOfRegister(Tokens[TC + 2].Value));
                            }

                            else
                            {
                                Console.WriteLine($"[ERROR] can not add {Tokens[TC].Type} with {Tokens[TC + 2].Type} on line {Tokens[TC].LineFound}");
                                Environment.Exit(1);
                            }

                            TC += 3;
                        }
                        break;
                    case "sub":
                        if (Tokens[TC].Type == TokenType.Opcode)
                        {
                            bytecode.Add(0x13);

                        }

                        else
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC + 1]);
                            if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Register)
                            {
                                bytecode.Add(0x12);

                                bytecode.Add(IdOfRegister(Tokens[TC].Value));
                                bytecode.Add(IdOfRegister(Tokens[TC + 2].Value));
                            }

                            else
                            {
                                Console.WriteLine($"[ERROR] can not subtract {Tokens[TC].Type} with {Tokens[TC + 2].Type} on line {Tokens[TC].LineFound}");
                                Environment.Exit(1);
                            }

                            TC += 3;
                        }
                        break;
                    case "mul":
                        if (Tokens[TC].Type == TokenType.Opcode)
                        {
                            bytecode.Add(0x15);
                        }

                        else
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC + 1]);
                            if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Register)
                            {
                                bytecode.Add(0x14);

                                bytecode.Add(IdOfRegister(Tokens[TC].Value));
                                bytecode.Add(IdOfRegister(Tokens[TC + 2].Value));
                            }

                            else
                            {
                                Console.WriteLine($"[ERROR] can not multiply {Tokens[TC].Type} with {Tokens[TC + 2].Type} on line {Tokens[TC].LineFound}");
                                Environment.Exit(1);
                            }

                            TC += 3;
                        }
                        break;
                    case "div":
                        if (Tokens[TC].Type == TokenType.Opcode)
                        {
                            bytecode.Add(0x17);

                        }

                        else
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC + 1]);
                            if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Register)
                            {
                                bytecode.Add(0x16);

                                bytecode.Add(IdOfRegister(Tokens[TC].Value));
                                bytecode.Add(IdOfRegister(Tokens[TC + 2].Value));
                            }

                            else
                            {
                                Console.WriteLine($"[ERROR] can not divide {Tokens[TC].Type} with {Tokens[TC + 2].Type} on line {Tokens[TC].LineFound}");
                                Environment.Exit(1);
                            }

                            TC += 3;
                        }
                        break;
                    case "not":
                        if (Tokens[TC].Type == TokenType.Opcode)
                        {
                            bytecode.Add(0x19);

                        }

                        else
                        {
                            if (Tokens[TC].Type == TokenType.Register)
                            {
                                bytecode.Add(0x18);

                                bytecode.Add(IdOfRegister(Tokens[TC].Value));
                            }

                            else
                            {
                                Console.WriteLine($"[ERROR] can not do NOT on {Tokens[TC].Type} on line {Tokens[TC].LineFound}");
                                Environment.Exit(1);
                            }

                            TC += 1;
                        }
                        break;
                    case "and":
                        if (Tokens[TC].Type == TokenType.Opcode)
                        {
                            bytecode.Add(0x1b);

                        }

                        else
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC + 1]);
                            if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Register)
                            {
                                bytecode.Add(0x1a);

                                bytecode.Add(IdOfRegister(Tokens[TC].Value));
                                bytecode.Add(IdOfRegister(Tokens[TC + 2].Value));
                            }

                            else
                            {
                                Console.WriteLine($"[ERROR] can not do AND on {Tokens[TC].Type} with {Tokens[TC + 2].Type} on line {Tokens[TC].LineFound}");
                                Environment.Exit(1);
                            }

                            TC += 3;
                        }
                        break;
                    case "or":
                        if (Tokens[TC].Type == TokenType.Opcode)
                        {
                            bytecode.Add(0x1d);

                        }

                        else
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC + 1]);
                            if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Register)
                            {
                                bytecode.Add(0x1c);

                                bytecode.Add(IdOfRegister(Tokens[TC].Value));
                                bytecode.Add(IdOfRegister(Tokens[TC + 2].Value));
                            }

                            else
                            {
                                Console.WriteLine($"[ERROR] can not do OR on {Tokens[TC].Type} with {Tokens[TC + 2].Type} on line {Tokens[TC].LineFound}");
                                Environment.Exit(1);
                            }

                            TC += 3;
                        }
                        break;
                    case "xor":
                        if (Tokens[TC].Type == TokenType.Opcode)
                        {
                            bytecode.Add(0x1f);

                        }

                        else
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC + 1]);
                            if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Register)
                            {
                                bytecode.Add(0x1e);

                                bytecode.Add(IdOfRegister(Tokens[TC].Value));
                                bytecode.Add(IdOfRegister(Tokens[TC + 2].Value));
                            }

                            else
                            {
                                Console.WriteLine($"[ERROR] can not do XOR on {Tokens[TC].Type} with {Tokens[TC + 2].Type} on line {Tokens[TC].LineFound}");
                                Environment.Exit(1);
                            }

                            TC += 3;
                        }
                        break;
                    case "syscall":
                        bytecode.Add(0xff);
                        break;
                    case "jmpa":
                        bytecode.Add(0x20);
                        TokenAssert(TokenType.Literal, Tokens[TC]);
                        {
                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);
                        }
                        TC += 1;
                        break;
                    case "jmpr":
                        bytecode.Add(0x21);
                        TokenAssert(TokenType.Literal, Tokens[TC]);

                        {
                            byte[] address = BitConverter.GetBytes(Convert.ToInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);
                        }
                        TC += 1;
                        break;
                    case "frs":
                        bytecode.Add(0x40);
                        break;
                    case "cmp":
                        if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Register)//16
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC + 1]);
                            bytecode.Add(0x30);
                            bytecode.Add(IdOfRegister(Tokens[TC].Value));
                            bytecode.Add(IdOfRegister(Tokens[TC+2].Value));
                            TC += 3;
                        }

                        else if (Tokens[TC].Type == TokenType.Register && Tokens[TC + 2].Type == TokenType.Literal)
                        {
                            TokenAssert(TokenType.Comma, Tokens[TC + 1]);
                            bytecode.Add(0x31);

                            bytecode.Add(IdOfRegister(Tokens[TC].Value));

                            byte[] literal = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC+2].Value));
                            Array.Reverse(literal);
                            bytecode.AddRange(literal);
                            TC += 3;
                        }

                        else if (Tokens[TC].Type == TokenType.Literal)
                        {
                            bytecode.Add(0x32);

                            byte[] literal = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(literal);
                            bytecode.AddRange(literal);
                            TC += 1;
                        }

                        else
                        {
                            if (Tokens[TC].Type == TokenType.Register)
                                TokenAssert(TokenType.Comma, Tokens[TC + 1]);

                            Console.WriteLine(Tokens[TC].Type != TokenType.Register ? $"[ERROR] can not have {Tokens[TC].Type} as first argument of cmp" : $"[ERROR] can not have {Tokens[TC+2].Type} as 2nd argument");
                            Environment.Exit(1);
                        }
                        
                        break;
                    case "je":
                        bytecode.Add(0x33);
                        TokenAssert(TokenType.Literal, Tokens[TC]);
                        {
                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);
                        }
                        TC += 1;
                        break;
                    case "jne":
                        bytecode.Add(0x34);
                        TokenAssert(TokenType.Literal, Tokens[TC]);
                        {
                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);
                        }
                        TC += 1;
                        break;
                    case "jg":
                        bytecode.Add(0x35);
                        TokenAssert(TokenType.Literal, Tokens[TC]);
                        {
                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);
                        }
                        TC += 1;
                        break;
                    case "js":
                        bytecode.Add(0x36);
                        TokenAssert(TokenType.Literal, Tokens[TC]);
                        {
                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);
                        }
                        TC += 1;
                        break;
                    case "jo":
                        bytecode.Add(0x37);
                        TokenAssert(TokenType.Literal, Tokens[TC]);
                        {
                            byte[] address = BitConverter.GetBytes(Convert.ToUInt32(Tokens[TC].Value));
                            Array.Reverse(address);
                            bytecode.AddRange(address);
                        }
                        TC += 1;
                        break;
                }
            }

            File.WriteAllBytes(FileName, bytecode.ToArray());
        }
    }
}
