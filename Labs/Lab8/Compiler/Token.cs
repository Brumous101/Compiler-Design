using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Compiler
{
    public class Token
    {
        public string Symbol;
        public string Lexeme;
        public int Line;
        public Token(string sym, string lexeme, int line)
        {
            this.Symbol = sym;
            this.Lexeme = lexeme;
            this.Line = line;
        }
        public override string ToString()
        {
            return $"[{this.Symbol} {this.Line} {this.Lexeme}]";
        }
    }

    public class Terminal
    {
        public string sym;
        public Regex rex;

        public Terminal(string sym, Regex rex)
        {
            this.sym = sym;
            this.rex = rex;
        }
    }
    public class Tokenizer
    {
        int idx;
        string input;
        int lineNum;
        int inputLength;
        List<Terminal> terminals = new List<Terminal>();
        List<Token> tokenStream = new List<Token>();
        Regex newLineRex;
        public Tokenizer(string gdata)
        {
            var lines = gdata.Split('\n');
            foreach (var line in lines)
            {   // EQ : :=  ->  ["EQ", ":=" ]
                try
                {
                    var twoHalves = Regex.Split(line, ":");
                    twoHalves[0] = twoHalves[0].Trim();
                    twoHalves[1] = twoHalves[1].Trim();
                    this.terminals.Add(new Terminal(twoHalves[0], new Regex(twoHalves[1].Trim())));
                    //Console.WriteLine("sym" + twoHalves[0] + " rex" + twoHalves[1]);
                }
                catch
                {
                    break;
                }
            }
            foreach (var token in this.terminals)
            {
                //Console.WriteLine(token.sym + " " + token.rex);
            }
        }
        public void setInput(string input)
        {
            
            this.idx = 0;
            this.input = input;
            this.inputLength = input.Length;
            this.lineNum = 0;
            this.newLineRex = new Regex(@"[\n\r]+");
            //Console.WriteLine("setINPUT:----->"+input);
        }
        public Token next()
        {
            //Console.WriteLine(input);
            //Console.WriteLine(input);
            if (this.idx >= inputLength)
            {
                //Console.WriteLine("EOF");
                tokenStream.Add(new Token("$", "", -1));
                return new Token("$", "", -1);
            }
            else
            {
                while (true)
                {
                    foreach (Terminal term in this.terminals)
                    {
                        Match m = term.rex.Match(input);
                        Match nL = newLineRex.Match(input);
                        if (this.idx >= inputLength)
                        {
                            //Console.WriteLine("EOF");
                            return new Token("$", "", -1);
                        }
                        if (m.Success)
                        {
                            if (m.Index == 0)
                            {
                                input = input.Remove(0, m.Length);
                                this.idx = this.idx + m.Length;
                                
                                if (term.sym == "WHITESPACE")
                                {
                                    return next();
                                }
                                else if (term.sym == "COMMENT")
                                {
                                    lineNum++;
                                    return next();
                                }
                                else
                                    tokenStream.Add(new Token(term.sym, m.Value, (this.lineNum + 1)));
                                    return new Token(term.sym, m.Value, (this.lineNum + 1));
                            }
                        }/*
                        if (nL.Success)
                        {
                            if (nL.Index == 0)
                            {
                                input = input.Remove(0, nL.Length);
                                this.idx = this.idx + nL.Length;
                                lineNum = lineNum + nL.Length;
                                return next();
                            }
                        }*/
                    }/*
                    foreach (Token token in tokenStream){
                        Console.WriteLine("Failure, returning tokens"+ token);
                    }*/
                    throw new InvalidOperationException("Out of Range Error");
                }
            }
        }
        public Boolean atEnd()
        {
            if (this.idx >= inputLength)
            {
                return true;
            }
            else
                return false;
        }
    }
}