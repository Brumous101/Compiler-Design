
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler;

namespace Compiler
{
    public static class Syms
    {
        public static Dictionary<int, string> intToString =
            new Dictionary<int, string>();
        public static Dictionary<string, int> stringToInt =
            new Dictionary<string, int>();
        public static void init(string tokenfile)
        {
            using (var fs = new StreamReader(tokenfile))
            {
                while (true)
                {
                    string s = fs.ReadLine();
                    if (s == null || s.Length == 0)
                        break;
                    string[] lst = s.Split(new char[] { '=' });
                    string name = lst[0];
                    int val = Int32.Parse(lst[1]);
                    intToString[val] = name;
                    stringToInt[name] = val;
                }
            }
        }
    }
    public class ASM
    {
        public List<string> instructions = new List<string>();
        public ASM()
        {
        }

        public ASM(string v1, string v2, string v3, string v4, ASM aSM, string v6)
        {
            this.instructions.Add(v1);
            this.instructions.Add(v2);
            this.instructions.Add(v3);
            this.instructions.Add(v4);
            this.instructions.AddRange(aSM.instructions);
            this.instructions.Add(v6);
        }

        public static ASM operator +(ASM a1, ASM a2)
        {
            ASM res = new ASM();
            res.instructions.AddRange(a1.instructions);
            res.instructions.AddRange(a2.instructions);
            return res;
        }

        public static implicit operator ASM(string s)
        {
            ASM res = new ASM();
            res.instructions.Add(s);
            return res;
        }

        public override string ToString()
        {
            //hacky: Try to make the indentation look nice
            string[] tmp = new string[instructions.Count];
            for (int i = 0; i < instructions.Count; ++i)
            {
                string instr = instructions[i];
                if (instr.Contains(":"))
                    tmp[i] = instr;
                else
                    tmp[i] = "    " + instr;
            }
            return String.Join("\n", tmp) + "\n";
        }
    }
    public class CodeStash : ParseTreeProperty<ASM>
    {
        public void Put(IParseTree context, params ASM[] asms)
        {
            ASM val = new ASM();
            foreach (ASM a in asms)
                val += a;
            base.Put(context, val);
        }
        public override ASM Get(IParseTree node)
        {
            ASM res = new ASM();
            if (base.Get(node) != null)
                return base.Get(node);
            else
            {
                for (int i = 0; i < node.ChildCount; ++i)
                {
                    var ch = node.GetChild(i);
                    res = res + this.Get(ch);
                }
            }
            return res;
        }
    }

    class CodeGenerator : ssuplBaseListener
    {
        public CodeStash code = new CodeStash();
        int labelCounter = 0;
        string label()
        {
            string tmp = $"lbl{labelCounter}";
            labelCounter++;
            return tmp;
        }
        public override void ExitFactor(ssuplParser.FactorContext context)
        {
            //factor -> NUM
            int d = Int32.Parse(context.NUM().GetText());
            code.Put(context, $"mov rax, {d}");
        }
        public override void ExitReturnStmt(ssuplParser.ReturnStmtContext context)
        {
            code.Put(context,
                    code.Get(context.expr()),
                    "ret");
        }
        public override void ExitCondElse(ssuplParser.CondElseContext context)
        {
            string endif = label();
            string elseblock = label();

            code.Put(context,
                $"; begin condElse at line {context.Start.Line}",
                     code.Get(context.expr()),
                     //"pop rax", not needed for this lab but nest lab you will need to PUSH and POP
                     "cmp rax, 0",
                     $"je {elseblock} ; Compare to value 0 if == 0 jump to else",
                     code.Get(context.braceblock(0)),
                     $"jmp {endif}",
                     $"{elseblock}: ; START OF ELSE",
                     code.Get(context.braceblock(1)),
                     $"{endif}:");
        }
        public override void ExitCondNoElse(ssuplParser.CondNoElseContext context)
        {
            string endif = label();
            code.Put(context,
                code.Get(context.expr()),
                "cmp rax,0",
                $"je {endif}",
                code.Get(context.braceblock()),
                $"{endif}:"
            );
        }
        public override void ExitLoop(ssuplParser.LoopContext context)
        {
            string endwhile = label();
            string startwhile = label();

            code.Put(context,
                $"; begin while loop at line yo {context.Start.Line}",
                     $"{startwhile}:",
                     code.Get(context.expr()),
                     "cmp rax, 0",
                     $"je {endwhile}",
                     code.Get(context.braceblock()),
                     $"jmp {startwhile}",
                     $"{endwhile}:");

        }
    }
    class MainClass
    {

        static string readFile(string f)
        {
            using (var fs = new StreamReader(f))
            {
                return fs.ReadToEnd();
            }
        }

        public static void Main(string[] args)
        {
            Syms.init("ssupl.tokens");

            string gdata;
            using (var r = new StreamReader("terminals.txt"))
            {
                gdata = r.ReadToEnd();
            }
            var tokenizer = new Tokenizer(gdata);

            string idata;

            using (var r = new StreamReader("input.txt"))
            {
                idata = r.ReadToEnd();
            }


            var rex = new Regex(@"\n[ \t]+([^\n]+)");

            idata = rex.Replace(idata, " $1");
            idata += "\n";

            tokenizer.setInput(idata);
            IList<IToken> tokens = new List<IToken>();
            while (true)
            {
                Token t = tokenizer.next();
                if (t.Symbol == "$")
                    break;
                CommonToken T = new CommonToken(Syms.stringToInt[t.Symbol], t.Lexeme);
                T.Line = t.Line;
                tokens.Add(T);
            }



            var antlrtokenizer = new BufferedTokenStream(new ListTokenSource(tokens));
            var parser = new ssuplParser(antlrtokenizer);
            parser.BuildParseTree = true;
            parser.ErrorHandler = new BailErrorStrategy();
            var antlrroot = parser.start();

            var listener = new CodeGenerator();
            var walker = new ParseTreeWalker();
            walker.Walk(listener, antlrroot);
            var allcode = new ASM(
                "default rel",
                "section .text",
                "global main",
                "main:",
                listener.code.Get(antlrroot),
                "section .data");
            using (var w = new StreamWriter("out.asm"))
            {
                w.Write(allcode.ToString());
            }
        }
    }
}

