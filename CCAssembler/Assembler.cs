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
            String,
            Undefined
        }
        
        private class Token
        {
            public string Value = "";
            public TokenType Type = TokenType.Undefined;
            public int LineFound = 0;
            public uint ByteIndex = 0;

            public void Print()
            {
                if (ByteIndex == 0)
                    Console.WriteLine($"Token[type={Type} Value={Value}]");
                else
                    Console.WriteLine($"Token[type={Type} Value={Value} ByteIndex={ByteIndex}]");
            }

            public Token Clone()
            {
                Token ret = new Token();
                ret.Value = Value;
                ret.Type = Type;
                ret.LineFound = LineFound;
                ret.ByteIndex = ByteIndex;
                return ret;
            }

            public void Reset()
            {
                Value = "";
                Type = TokenType.Undefined;
                ByteIndex = 0;
                LineFound = 0;
            }
        }

        private string Assembly;
        private List<Token> Tokens = new List<Token>();
        private bool CommentMode = false;
        private bool LabelMode = false;
        private bool StringMode = false;
        uint ByteIndexCounter = 0;

        public void LoadAssembly(string assembly)
        {
            Assembly = assembly.Replace("\t", "").Replace("\r","");
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

                if (StringMode)
                {
                    if (CurrChar == '\"')
                    {
                        StringMode = false;
                        if (CurrTok.Value != "")
                        {
                            CurrTok.LineFound = LineCount;
                            Tokens.Add(CurrTok.Clone());
                            CurrTok.Reset();
                        }
                    }

                    else
                        CurrTok.Value += CurrChar;
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

                        if (CurrTok.Type == TokenType.Literal)
                            ByteIndexCounter += 4;

                        CurrTok.LineFound = LineCount;
                        Tokens.Add(CurrTok.Clone());
                        CurrTok.Reset();
                    }
                }

                else if (CurrChar == '\"')
                {
                    StringMode = true;
                    CurrTok.Type = TokenType.String;
                    continue;
                }

                else if ((CurrChar == ' ' || CurrChar == '\n') && CurrTok.Value != "" && !LabelMode)
                {
                    if (CurrTok.Type == TokenType.Opcode && (CurrTok.Value == "a" || CurrTok.Value == "b" || CurrTok.Value == "c" || CurrTok.Value == "d"))
                    {
                        CurrTok.Type = TokenType.Register;
                    }

                    if (CurrTok.Type == TokenType.Literal)
                        ByteIndexCounter += 4;

                    CurrTok.LineFound = LineCount;
                    Tokens.Add(CurrTok.Clone());
                    CurrTok.Reset();

                    continue;
                }

                else if (CurrChar == ' ' && !LabelMode)
                    continue;

                else if (Char.IsLetter(CurrChar) && (CurrTok.Type == TokenType.Undefined || CurrTok.Type == TokenType.Opcode) && !LabelMode)
                {

                    if (CurrTok.Value.Length == 0)
                        ByteIndexCounter++;

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
                    if (CurrTok.Value.Length == 0)
                        ByteIndexCounter += 4;
                    CurrTok.Type = TokenType.Address;
                }

                else if (CurrChar == ':' || LabelMode)
                {
                    LabelMode = true;

                    if (CurrChar == ' ' || CurrChar == '\n')
                    {
                        CurrTok.Type = TokenType.Label;
                        CurrTok.LineFound = LineCount;
                        CurrTok.ByteIndex = ByteIndexCounter;
                        Tokens.Add(CurrTok.Clone());
                        CurrTok.Reset();

                        LabelMode = false;
                    }

                    else
                    {
                        CurrTok.Type = TokenType.Label;
                        CurrTok.Value += CurrChar;
                    }
                }

                else if (CurrChar == '\n')
                    continue;

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
            ParseLabels();
            ParseDefs();
            ReplaceLabels();
            
        }

        private Dictionary<string, uint> Labels = new Dictionary<string, uint>();
        private void ParseLabels()
        {
            for (int i = 0; i < Tokens.Count; i++)
            {
                Token T = Tokens[i];
                if (T.Type == TokenType.Label)
                {
                    Tokens.RemoveAt(i);
                    Labels.Add(T.Value.Substring(1), T.ByteIndex);
                }
            }
        }

        private string[] ValidOpcodes = {
            "stp", "psh", "pop", "dup", "mov", "add", "sub", "mul",
            "div", "not", "and", "or", "xor", "cmp", "je", "jne",
            "jg", "js", "jo", "frs", "syscall", "jmpa", "jmpr"
        };

        private Dictionary<String, (int, string)> defintions = new Dictionary<String, (int, string)>();
        private void ParseDefs()
        {
            int currPointer = 0;
            for (int i = 0; i < Tokens.Count; i++)
            {
                Token T = Tokens[i];
                if (T.Type == TokenType.Opcode && T.Value == "def")
                {
                    TokenAssert(TokenType.Opcode, Tokens[i + 1]);
                    TokenAssert(TokenType.String, Tokens[i + 2]);

                    defintions.Add(Tokens[i + 1].Value, (currPointer, Tokens[i + 2].Value));
                    currPointer += Tokens[i + 2].Value.Length;

                    int indx = -1;
                    Tokens.RemoveAll(T => { indx++;  return indx == i || indx == i + 1 || indx == i + 2; });
                }
            }
        }

        private void ReplaceLabels()
        {
            for (int i = 0; i < Tokens.Count; i++)
            {
                Token T = Tokens[i];
                if (T.Type == TokenType.Opcode && Labels.ContainsKey(T.Value))
                {
                    if (Array.Exists<string>(ValidOpcodes, (string value) => { return value == T.Value; }))
                    {
                        Console.WriteLine($"[ERROR] unexisting label on line {T.LineFound}: {T.Value}");
                        Environment.Exit(1);
                    }else
                    {
                        Tokens[i].Type = TokenType.Literal;
                        Tokens[i].Value = Labels[Tokens[i].Value].ToString();
                    }
                }

                else if (T.Type == TokenType.Opcode && defintions.ContainsKey(T.Value))
                {
                    if (Array.Exists<string>(ValidOpcodes, (string value) => { return value == T.Value; }))
                    {
                        Console.WriteLine($"[ERROR] unexisting data pointer on line {T.LineFound}: {T.Value}");
                        Environment.Exit(1);
                    } else
                    {
                        Tokens[i].Type = TokenType.Literal;
                        Tokens[i].Value = defintions[Tokens[i].Value].Item1.ToString();
                    }
                }
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

        public void GenerateHeader(ref List<byte> bytecode)
        {
            // memory reserving
            byte[] memsize = { 0x00, 0x00, 0x0e, 0x38 };
            bytecode.AddRange(memsize);

            // generate headerbytes
            foreach (var d in defintions)
            {
                foreach (char c in d.Value.Item2)
                {
                    bytecode.Add((byte)c);
                }
            }

            // header ending
            byte[] ending = { 0x1d, 0x1d, 0x1d, 0x1d };
            bytecode.AddRange(ending);
        }

        public void GenerateCode(string FileName)
        {
            List<byte> bytecode = new List<byte>();
            GenerateHeader(ref bytecode);
            

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
