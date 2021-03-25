using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace parser
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
        List<Token> tokenList;
        List<Terminal> terminals = new List<Terminal>();
        string trimmed;
        string[] clonePut;
        string noSpacesWithNewLine;
        Boolean linesAccFor;
        Boolean matchWasDone;
        Regex newLineRex;
        Regex whiteSpaceRex;
        int counter;
        public Tokenizer(string gdata)
        {
            var lines = gdata.Split('\n');
            foreach (var line in lines)
            {   // EQ : :=  ->  ["EQ", ":=" ]
                var twoHalves = Regex.Split(line, ":");
                this.terminals.Add(new Terminal(twoHalves[0].Trim(), new Regex(twoHalves[1].Trim())));
            }
            foreach (var token in this.terminals)
            {
                Console.WriteLine(token.sym + " " + token.rex);
            }
        }
        public void setInput(string input)
        {
            this.idx = 0;
            this.counter = 0;
            this.input = input;
            this.inputLength = input.Length;
            this.tokenList = new List<Token>();
            this.noSpacesWithNewLine = Regex.Replace(input.Trim(), @"[^\S\r\n]", "");
            this.trimmed = String.Concat(input.Where(c => !Char.IsWhiteSpace(c)));
            this.clonePut = this.trimmed.Split('\n');
            this.lineNum = 0;
            this.linesAccFor = false;
            this.newLineRex = new Regex(@"[\n\r]+");
            this.whiteSpaceRex = new Regex(@"[\s]");
            this.matchWasDone = false;
        }
        public Token next()
        {
            //this.counter = 0;
            if (this.idx >= inputLength)
            {
                Console.WriteLine("EOF");
                //this.tokenList.Add(new Token("$", "", -1));
                return new Token("$", "", -1);
            }
            else
            {
                foreach (Terminal term in this.terminals)
                {
                    //Match m = term.rex.Match(input,this.idx);
                    Match m = term.rex.Match(input);
                    if (m.Success)
                    {
                        if (m.Index == 0)
                        {
                            input = input.Remove(0, m.Length);
                            this.idx = this.idx + m.Length;
                            if (term.sym == "WHITESPACE")
                            {
                                return next();
                                //this.tokenList.Add(new Token(term.sym, m.Value, (this.lineNum + 1)));
                            }
                            //JH
                            else
                                return new Token(term.sym, m.Value, (this.lineNum + 1));
                            //this.counter = 0;
                        }
                    }
                }
                throw new InvalidOperationException("Out of Range Error");
            }
            Console.WriteLine("returning temps");
            if (this.tokenList.FindIndex(Token => Token.Symbol == "$") >= 0)
            {

                int comments = this.tokenList.Count(Token => Token.Symbol == "COMMENT");
                //Console.WriteLine("comment count" + comments);
                while (comments > 0)
                {
                    this.tokenList.RemoveAt(this.tokenList.FindIndex(Token => Token.Symbol == "COMMENT"));
                    comments--;
                }
                Console.WriteLine("Token Popped: " + this.tokenList.First());
                Token temp = tokenList[0];
                if (this.tokenList[0].Symbol == "$")
                    this.idx = 0;
                this.tokenList.RemoveAt(0);
                return temp;
            }
        }
    }
}