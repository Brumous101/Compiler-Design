
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
        public override void ExitCondNoElse(ssuplParser.CondNoElseContext context)
        {
            //cond : IF LP expr RP braceblock 
            string endif = label();
            code.Put(context,
                code.Get(context.expr()),
                "pop rax",
                "cmp rax,0",
                $"je {endif}",
                code.Get(context.braceblock()),
                $"{endif}:"
            );
        }
        public override void ExitCondElse(ssuplParser.CondElseContext context)
        {
            //cond : IF LP expr RP braceblock ELSE braceblock
            string endif = label();
            string elseblock = label();

            code.Put(context,
                $"; begin condElse at line {context.Start.Line}",
                     code.Get(context.expr()),
                     "pop rax", //not needed for this lab but nest lab you will need to PUSH and POP
                     "cmp rax, 0",
                     $"je {elseblock} ; Compare to value 0 if == 0 jump to else",
                     code.Get(context.braceblock(0)),
                     $"jmp {endif}",
                     $"{elseblock}: ; START OF ELSE",
                     code.Get(context.braceblock(1)),
                     $"{endif}:");
        }
        public override void ExitReturnStmt(ssuplParser.ReturnStmtContext context)
        {
            // returnStmt : RETURN expr
            code.Put(context,
                code.Get(context.expr()),
                "pop rax",
                "ret");
        }
        public override void ExitLoop(ssuplParser.LoopContext context)
        {
            string endwhile = label();
            string startwhile = label();

            code.Put(context,
                $"; begin while loop at line yo {context.Start.Line}",
                     $"{startwhile}:",
                     code.Get(context.expr()),
                     "pop rax",
                     "cmp rax, 0",
                     $"je {endwhile}",
                     code.Get(context.braceblock()),
                     $"jmp {startwhile}",
                     $"{endwhile}:");

        }
        public override void ExitOrexp1(ssuplParser.Orexp1Context context)
        {
            //orexp  : orexp OR andexp    #orexp1
            var firstWasFalse = label();
            var done = label();
            code.Put(context,
                code.Get(context.orexp()),  //first term
                "pop rax",              //result of first term
                "cmp rax,0",            //see if it's zero
                $"je {firstWasFalse}",  //if so, do second test
                "push qword 1",         //if not, push 1 to stack
                $"jmp {done}",          //skip second test
                $"{firstWasFalse}:",    //start of second test
                code.Get(context.andexp()), //second term
                "pop rax",              //result of second term
                "cmp rax,0",            //see if it's zero
                "setne al",             //if not, set al to 1
                "movzx qword rax,al",  //zero extend
                "push rax",             //push result
                $"{done}:"
            );
        }
        public override void ExitAndexp1(ssuplParser.Andexp1Context context)
        {
            //andexp  : andexp AND notexp
            var firstWasTrue = label();
            var done = label();
            code.Put(context,
                code.Get(context.andexp()),  //first term
                "pop rax",              //result of first term
                "cmp rax,0",            //see if it's zero
                $"jne {firstWasTrue}",           //if so, end
                "push qword 0",
                $"jmp {done}",          //skip second test
                $"{firstWasTrue}:",    //start of second test
                code.Get(context.notexp()), //second term
                "pop rax",              //result of second term
                "cmp rax,0",            //see if it's zero
                "setne al",             //al gets to set to either 1 or 0
                "movzx qword rax,al",   //clear all values but the 1 or 0
                "push rax",             //push result
                $"{done}:"
            );
        }
        public override void ExitNotexp1(ssuplParser.Notexp1Context context)
        {
            //notexp  : NOT notexp
            code.Put(context,
                code.Get(context.notexp()), //second term
                    "pop rax",              //result of second term
                    "cmp rax,0",            //see if it's zero
                    "sete al",              //if not, set al to 1
                    "movzx qword rax,al",   //zero extend
                    "push rax"              //push result
                );
        }
        public override void ExitRel1(ssuplParser.Rel1Context context)
        {
            //rel    : sum RELOP sum
            string mnemonic;
            switch (context.RELOP().GetText())
            {
                case ">=": mnemonic = "setge"; break;
                case "<=": mnemonic = "setle"; break;
                case ">": mnemonic = "setg"; break;
                case "<": mnemonic = "setl"; break;
                case "==": mnemonic = "sete"; break;
                case "!=": mnemonic = "setne"; break;
                default: throw new Exception();
            }
            code.Put(context,
                code.Get(context.sum()[0]),
                code.Get(context.sum()[1]),
                "pop rbx",      //second operand
                "pop rax",      //first operand
                "cmp rax,rbx",  //do the compare
                $"{mnemonic} al",   //set low byte of rax (0 or 1)
                "movzx qword rax,al",   //zero extend
                "push rax"
            );
        }
        public override void ExitSum1(ssuplParser.Sum1Context context)
        {
            //sum    : sum PLUS term      #sum1
            code.Put(context,
                code.Get(context.sum()),
                code.Get(context.term()),
                "pop rbx",      //value from term
                "pop rax",      //value from sum
                "add rax,rbx",
                "push rax"
            );
        }
        public override void ExitSum2(ssuplParser.Sum2Context context)
        {
            //sum    : sum MINUS term      #sum2
            code.Put(context,
                code.Get(context.sum()),
                code.Get(context.term()),
                "pop rbx",      //value from term
                "pop rax",      //value from sum
                "sub rax,rbx",
                "push rax"
            );
        }
        public override void ExitTerm1(ssuplParser.Term1Context context)
        {
            //term    :  term MULOP neg
            if (context.MULOP().GetText() == "*")
            {
                //Console.WriteLine("made it here *");
                code.Put(context,
                    code.Get(context.term()),
                    code.Get(context.neg()),
                    "pop rbx",      //value from term
                    "pop rax",      //value from neg
                    "imul rax,rbx",
                    "push rax"
                );
            }
            else if (context.MULOP().GetText() == "/")
            {
                //Console.WriteLine("made it here /");
                code.Put(context,
                    code.Get(context.term()),
                    code.Get(context.neg()),
                    "pop rbx",      //value from term
                    "pop rax",      //value from neg
                    "mov rdx,0",    //Must clear the divisor register
                    "idiv rbx",
                    "push rax"
                );
            }
            else //Modulo
            {
                //Console.WriteLine("made it here /");
                code.Put(context,
                    code.Get(context.term()),
                    code.Get(context.neg()),
                    "pop rbx",      //value from term
                    "pop rax",      //value from neg
                    "mov rdx,0",    //Must clear the divisor register
                    "idiv rbx",
                    "push rdx"
                );
            }
        }
        public override void ExitNeg1(ssuplParser.Neg1Context context)
        {
            //neg    :  MINUS neg
            code.Put(context,
                code.Get(context.neg()),
                "pop rax",      //value from neg
                "neg rax",
                "push rax"
            );
        }
        public override void ExitFactor1(ssuplParser.Factor1Context context)
        {
            //factor -> NUM
            var d = Int32.Parse(context.NUM().GetText());
            code.Put(context,
                $"push qword {d}");
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

