
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler;

namespace Compiler
{
    public enum VarType
    {
        INVALID_TYPE = 0, INT, DOUBLE, STRING, VEC4
    };

    public class VarInfo
    {
        public VarType type;
        public string location;
        public int line;
        public VarInfo(VarType t, string loc, int ln)
        {
            type = t;
            location = loc;
            line = ln;
        }
    }
    public class SymbolTable
    {
        public Dictionary<string, VarInfo> table = new Dictionary<string, VarInfo>();
        public SymbolTable()
        {
        }
        public VarInfo Get(string name)
        {
            if (!table.ContainsKey(name))
                throw new Exception("Name " + name + " does not exist");
            return table[name];
        }
        public void Set(string name, VarInfo v)
        {
            if (table.ContainsKey(name))
                throw new Exception("Redeclaration of " + name);
            table[name] = v;
        }
        public bool Contains(string name)
        {
            return table.ContainsKey(name);
        }
    }
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

        public ASM(string v1, string v2)
        {
            this.instructions.Add(v1);
            this.instructions.Add(v2);
        }
        public ASM(string v1, string v2, string v3, string v4, string v5)
        {
            this.instructions.Add(v1);
            this.instructions.Add(v2);
            this.instructions.Add(v3);
            this.instructions.Add(v4);
            this.instructions.Add(v5);
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
        public SymbolTable symtable = new SymbolTable();
        public CodeStash code = new CodeStash();
        public ParseTreeProperty<VarType> typeAttr = new ParseTreeProperty<VarType>();
        public Dictionary<string, StringInfo> stringPool = new Dictionary<string, StringInfo>();
        VarType stringToType(string s)
        {
            Type t = typeof(VarType);
            return (VarType)Enum.Parse(t, s.ToUpper());
        }
        int labelCounter = 0;
        string label()
        {
            string tmp = $"lbl{labelCounter}";
            labelCounter++;
            return tmp;
        }
        VarType type(IParseTree context)
        {
            IParseTree n = context;
            while (true)
            {
                if (typeAttr.Get(n) != VarType.INVALID_TYPE)
                    return typeAttr.Get(n);
                if (n.ChildCount != 1)
                    throw new Exception("ICE");
                n = n.GetChild(0);
            }
        }
        int SizeForType(VarType v)
        {
            switch (v)
            {
                case VarType.INT:
                case VarType.DOUBLE:
                case VarType.STRING:
                    return 8;
                case VarType.VEC4:
                    return 32;
                default:
                    throw new Exception("ICE");
            }
        }
        public override void ExitCondNoElse(ssuplParser.CondNoElseContext context)
        {
            //cond : IF LP expr RP braceblock 
            //new
            if (type(context.expr()) != VarType.INT)
                throw new Exception("Condition must be integer type");
            //end new

            string endif = label();
            code.Put(context,
                $"; begin condNoElse at line {context.Start.Line}",
                code.Get(context.expr()),
                "pop rax",
                "cmp rax,0",
                $"je {endif}",
                $";begin condNoElse braceblock",
                code.Get(context.braceblock()),
                $"{endif}:",
                $"; end condNoElse at line {context.Stop.Line}"

            );
        }
        public override void ExitCondElse(ssuplParser.CondElseContext context)
        {
            //cond : IF LP expr RP braceblock ELSE braceblock
            string endif = label();
            string elseblock = label();
            if (type(context.expr()) != VarType.INT)
                throw new Exception("Expr must be integer");
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
            if (type(context.expr()) != VarType.INT)
                throw new Exception("Expr must be integer");
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
            //orexp  -> orexp OR andexp    #orexp1
            if (type(context.orexp()) != VarType.INT)
                throw new Exception("First operand to OR must be integer");
            if (type(context.andexp()) != VarType.INT)
                throw new Exception("Second operand to OR must be integer");
            typeAttr.Put(context, VarType.INT);
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
            if (type(context.andexp()) != VarType.INT)
                throw new Exception("First operand to OR must be integer");
            if (type(context.notexp()) != VarType.INT)
                throw new Exception("Second operand to OR must be integer");
            typeAttr.Put(context, VarType.INT);
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
            if (type(context.notexp()) != VarType.INT)
                throw new Exception("First operand to OR must be integer");
            typeAttr.Put(context, VarType.INT);
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
            if (type(context.sum()[0]) != type(context.sum()[1]))
                throw new Exception("Type mismatch");
            if (type(context.sum()[0]) == VarType.VEC4 || type(context.sum()[1]) == VarType.VEC4)
                throw new Exception("Can't compared VEC4 not field types");
            if (type(context.sum()[0]) == VarType.STRING || type(context.sum()[1]) == VarType.STRING)
                throw new Exception("Can't compared STRING not field types");
            typeAttr.Put(context, VarType.INT);
            string mnemonic;
            int op;
            switch (context.RELOP().GetText())
            {
                case ">=": mnemonic = "setge"; op = 5; break;
                case "<=": mnemonic = "setle"; op = 2; break;
                case ">": mnemonic = "setg"; op = 6; break;
                case "<": mnemonic = "setl"; op = 1; break;
                case "==": mnemonic = "sete"; op = 0; break;
                case "!=": mnemonic = "setne"; op = 4; break;
                default: throw new Exception();
            }
            switch (type(context.sum()[0]))
            {
                case VarType.INT:
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
                    break;
                case VarType.DOUBLE:
                    code.Put(context,
                        code.Get(context.sum()[0]),
                        code.Get(context.sum()[1]),
                        "movq xmm1, [rsp]", //pop value...
                        "add rsp,8",        //...from second operand
                        "movq xmm0, [rsp]", //pop value...
                        "add rsp,8",        //...from first operand
                        $"cmpsd xmm0, xmm1, {op}",
                        "movq rax,xmm0",    //need to convert to 0 or 1
                        "and rax,1",
                        "push rax"
                    );
                    break;
            }
        }
        public override void ExitSum1(ssuplParser.Sum1Context context)
        {
            //sum    : sum PLUS term      #sum1

            //Console.WriteLine("context sum" + type(context.sum()) + "context term" + type(context.term()));
            if (type(context.sum()) != type(context.term()))
                throw new Exception("Type mismatch");
            typeAttr.Put(context, type(context.sum()));
            switch (type(context.sum()))
            {
                case VarType.INT:
                    code.Put(context,
                        code.Get(context.sum()),
                        code.Get(context.term()),
                        "pop rbx",      //value from term
                        "pop rax",      //value from sum
                        "add rax,rbx",
                        "push rax"
                    );
                    break;
                case VarType.DOUBLE:
                    code.Put(context,
                        code.Get(context.sum()),
                        code.Get(context.term()),
                        "movq xmm1, [rsp]", //pop value...
                        "add rsp,8",        //...from term
                        "movq xmm0, [rsp]", //pop value...
                        "add rsp,8",        //...from sum
                        "addsd xmm0,xmm1",  //xmm0 <- xmm0+xmm1
                        "sub rsp,8",        //do the push...
                        "movq [rsp], xmm0"  //...of the result
                    );
                    break;
                case VarType.VEC4:
                    code.Put(context,
                        code.Get(context.sum()),
                        code.Get(context.term()),
                        "movq xmm4, [rsp]",     //term.x
                        "movq xmm0, [rsp+32]",  //sum.x
                        "addsd xmm0,xmm4",      //result.x
                        "movq xmm4, [rsp+8]",   //term.y
                        "movq xmm1, [rsp+40]",  //sum.y
                        "addsd xmm1,xmm4",      //result.y
                        "movq xmm4, [rsp+16]",  //term.z
                        "movq xmm2, [rsp+48]",  //sum.z
                        "addsd xmm2,xmm4",      //result.z
                        "movq xmm4, [rsp+24]",  //term.w
                        "movq xmm3, [rsp+56]",  //sum.w
                        "addsd xmm3,xmm4",      //result.w
                        "add rsp,32",   //pop 64 bytes, push 32
                        "movq [rsp], xmm0",     //result.x
                        "movq [rsp+8], xmm1",   //result.y
                        "movq [rsp+16], xmm2",  //result.z
                        "movq [rsp+24], xmm3"   //result.w
                    );
                    break;
                default:
                    throw new Exception("Bad type for summation");
            }
        }
        public override void ExitSum2(ssuplParser.Sum2Context context)
        {
            //sum    : sum MINUS term      #sum2
            if (type(context.sum()) != type(context.term()))
                throw new Exception("Type mismatch");
            typeAttr.Put(context, type(context.sum()));
            switch (type(context.sum()))
            {
                case VarType.INT:
                    code.Put(context,
                        code.Get(context.sum()),
                        code.Get(context.term()),
                        "pop rbx",      //value from term
                        "pop rax",      //value from sum
                        "sub rax,rbx",
                        "push rax"
                    );
                    break;
                case VarType.DOUBLE:
                    code.Put(context,
                        code.Get(context.sum()),
                        code.Get(context.term()),
                        "movq xmm1, [rsp]", //pop value...
                        "add rsp,8",        //...from term
                        "movq xmm0, [rsp]", //pop value...
                        "add rsp,8",        //...from sum
                        "subsd xmm0,xmm1",  //xmm0 <- xmm0-xmm1
                        "sub rsp,8",        //do the push...
                        "movq [rsp], xmm0"  //...of the result
                    );
                    break;
                case VarType.VEC4:
                    code.Put(context,
                        code.Get(context.sum()),
                        code.Get(context.term()),
                        "movq xmm4, [rsp]",     //term.x
                        "movq xmm0, [rsp+32]",  //sum.x
                        "subsd xmm0,xmm4",      //result.x
                        "movq xmm4, [rsp+8]",   //term.y
                        "movq xmm1, [rsp+40]",  //sum.y
                        "subsd xmm1,xmm4",      //result.y
                        "movq xmm4, [rsp+16]",  //term.z
                        "movq xmm2, [rsp+48]",  //sum.z
                        "subsd xmm2,xmm4",      //result.z
                        "movq xmm4, [rsp+24]",  //term.w
                        "movq xmm3, [rsp+56]",  //sum.w
                        "subsd xmm3,xmm4",      //result.w
                        "add rsp,32",   //pop 64 bytes, push 32
                        "movq [rsp], xmm0",     //result.x
                        "movq [rsp+8], xmm1",   //result.y
                        "movq [rsp+16], xmm2",  //result.z
                        "movq [rsp+24], xmm3"   //result.w
                    );
                    break;
            }
        }
        public override void ExitTerm1(ssuplParser.Term1Context context)
        {
            //term    :  term MULOP neg
            if (type(context.term()) != type(context.neg()))
                throw new Exception("Type mismatch");
            typeAttr.Put(context, type(context.term()));
            switch (type(context.term()))
            {
                case VarType.INT:
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
                    break;
                case VarType.DOUBLE:
                    if (context.MULOP().GetText() == "*")
                    {
                        //Console.WriteLine("made it here *");
                        code.Put(context,
                            code.Get(context.term()),
                            code.Get(context.neg()),
                            "movq xmm0, [rsp]",      //pop double from stack from term
                            "add rsp,8",
                            "movq xmm1, [rsp]",      //pop double from stack from neg
                            "add rsp,8",
                            "mulsd xmm0,xmm1",
                            "sub rsp,8",
                            "movq [rsp], xmm0" //push back on to stack
                        );
                    }
                    else if (context.MULOP().GetText() == "/")
                    {
                        //Console.WriteLine("made it here /");
                        code.Put(context,
                            code.Get(context.term()),
                            code.Get(context.neg()),
                            "movq xmm0, [rsp]",      //pop double from stack from term
                            "add rsp,8",
                            "movq xmm1, [rsp]",      //pop double from stack from neg
                            "add rsp,8",
                            "divsd xmm1,xmm0",
                            "sub rsp,8",
                            "movq [rsp], xmm1" //push back on to stack
                        );
                    }
                    else //Modulo
                    {
                        throw new Exception("Modulo not possible on double type");
                    }
                    break;
                case VarType.VEC4:
                    if (context.MULOP().GetText() == "*")
                    {
                        //Console.WriteLine("made it here *");
                        code.Put(context,
                            code.Get(context.term()),
                            code.Get(context.neg()),
                            "movq xmm4, [rsp]",     //term.x
                            "movq xmm0, [rsp+32]",  //sum.x
                            "mulsd xmm0,xmm4",      //result.x
                            "movq xmm4, [rsp+8]",   //term.y
                            "movq xmm1, [rsp+40]",  //sum.y
                            "mulsd xmm1,xmm4",      //result.y
                            "movq xmm4, [rsp+16]",  //term.z
                            "movq xmm2, [rsp+48]",  //sum.z
                            "mulsd xmm2,xmm4",      //result.z
                            "movq xmm4, [rsp+24]",  //term.w
                            "movq xmm3, [rsp+56]",  //sum.w
                            "mulsd xmm3,xmm4",      //result.w
                            "add rsp,32",   //pop 64 bytes, push 32
                            "movq [rsp], xmm0",     //result.x
                            "movq [rsp+8], xmm1",   //result.y
                            "movq [rsp+16], xmm2",  //result.z
                            "movq [rsp+24], xmm3"   //result.w
                        );
                    }
                    else if (context.MULOP().GetText() == "/")
                    {
                        //Console.WriteLine("made it here /");
                        code.Put(context,
                            code.Get(context.term()),
                            code.Get(context.neg()),
                            "movq xmm4, [rsp]",     //term.x
                            "movq xmm0, [rsp+32]",  //sum.x
                            "divsd xmm0,xmm4",      //result.x
                            "movq xmm4, [rsp+8]",   //term.y
                            "movq xmm1, [rsp+40]",  //sum.y
                            "divsd xmm1,xmm4",      //result.y
                            "movq xmm4, [rsp+16]",  //term.z
                            "movq xmm2, [rsp+48]",  //sum.z
                            "divsd xmm2,xmm4",      //result.z
                            "movq xmm4, [rsp+24]",  //term.w
                            "movq xmm3, [rsp+56]",  //sum.w
                            "divsd xmm3,xmm4",      //result.w
                            "add rsp,32",   //pop 64 bytes, push 32
                            "movq [rsp], xmm0",     //result.x
                            "movq [rsp+8], xmm1",   //result.y
                            "movq [rsp+16], xmm2",  //result.z
                            "movq [rsp+24], xmm3"   //result.w
                        );
                    }
                    else //Modulo
                    {
                        throw new Exception("Modulo not possible on vec4 type");
                    }
                    break;
            }
            
        }
        public override void ExitNeg1(ssuplParser.Neg1Context context)
        {
            //neg    :  MINUS neg
            typeAttr.Put(context, type(context.neg()));

            switch (type(context.neg()))
            {
                case VarType.INT:
                    code.Put(context,
                        code.Get(context.neg()),
                        "pop rax",      //value from neg
                        "neg rax",
                        "push rax"
                    );
                    break;
                case VarType.DOUBLE:
                    code.Put(context,
                        code.Get(context.neg()),
                        "movq xmm1, [rsp]", //pop value
                        "xorpd xmm0,xmm0",  //fast 0
                        "subsd xmm0,xmm1",  //negate
                        "movq [rsp], xmm0"  //push result
                    );
                    break;
                case VarType.VEC4:
                    //Console.WriteLine("made it here *");
                    code.Put(context,
                        code.Get(context.neg()),
                        "movq xmm1, [rsp]", //pop value
                        "xorpd xmm0,xmm0",  //fast 0
                        "subsd xmm0,xmm1",  //negate
                        "movq [rsp], xmm0",  //push result

                        "movq xmm1, [rsp+8]", //pop value
                        "xorpd xmm0,xmm0",  //fast 0
                        "subsd xmm0,xmm1",  //negate
                        "movq [rsp+8], xmm0",  //push result

                        "movq xmm1, [rsp+16]", //pop value
                        "xorpd xmm0,xmm0",  //fast 0
                        "subsd xmm0,xmm1",  //negate
                        "movq [rsp+16], xmm0",  //push result

                        "movq xmm1, [rsp+24]", //pop value
                        "xorpd xmm0,xmm0",  //fast 0
                        "subsd xmm0,xmm1",  //negate
                        "movq [rsp+24], xmm0"  //push result
                    );
                    break;
            }
        }
        public override void ExitFactor1(ssuplParser.Factor1Context context)
        {
            //factor -> NUM
            var d = Int32.Parse(context.NUM().GetText());
            code.Put(context,
                $"push qword {d}"
            );
            typeAttr.Put(context, VarType.INT);    //NEW!
        }
        public override void ExitFactor2(ssuplParser.Factor2Context context)
        {
            //factor -> LP expr RP
            typeAttr.Put(context, type(context.expr()));
        }
        public override void ExitFactor3(ssuplParser.Factor3Context context)
        {
            //factor -> ID
            string vname = context.ID().GetText();
            if (!symtable.Contains(vname))
                throw new Exception("Undeclared variable: " + vname);
            string lbl = symtable.Get(vname).location;
            VarType vtype = symtable.Get(vname).type;

            int vsizeInQuads = SizeForType(vtype) / 8;
            ASM asmcode = new ASM();
            for (int i = vsizeInQuads - 1; i >= 0; i--)
            {
                asmcode += $"mov rax,[{lbl}+{i * 8}]";
                asmcode += "push qword rax";
            }
            code.Put(context, asmcode);

            typeAttr.Put(context, symtable.Get(vname).type);
        }
        public override void ExitFactor4(ssuplParser.Factor4Context context)
        {
            //factor -> FPNUM
            double d = Double.Parse(context.FPNUM().GetText());
            string ds = "" + d;
            if (!ds.Contains("."))
                ds += ".0";
            code.Put(context,
                $"mov rax, __float64__({ds})",
                "push rax"
            );
            typeAttr.Put(context, VarType.DOUBLE);
        }
        public override void ExitFactor6(ssuplParser.Factor6Context context)
        {
            //factor -> STRINGCONST
            var s = context.STRINGCONST().GetText();
            //...strip leading and trailing quotation marks...
            //...handle backslash escapes...
            if (!stringPool.ContainsKey(s))
                stringPool[s] = new StringInfo(label());
            code.Put(context,
                $"mov rax, {stringPool[s].address}",
                "push rax"
            );
            typeAttr.Put(context, VarType.STRING);
        }
        public override void ExitFactor7(ssuplParser.Factor7Context context)
        {
            //factor -> ID FIELD
            string vname = context.ID().GetText();
            if (!symtable.Contains(vname))
                throw new Exception("Undeclared variable: " + vname);
            if (symtable.Get(vname).type != VarType.VEC4)
                throw new Exception("Type not a vec4 inside of ExitFactor7");
            VarType vtype = symtable.Get(vname).type;
            int vsizeInQuads = SizeForType(vtype) / 8;
            ASM asmcode = new ASM();
            var lbl = symtable.Get(vname).location;
            switch(context.FIELD().GetText())
            {
                case ".x":
                    asmcode += $"mov rax,[{lbl}+{0}]";
                    break;
                case ".y":
                    asmcode += $"mov rax,[{lbl}+{8}]";
                    break;
                case ".z":
                    asmcode += $"mov rax,[{lbl}+{16}]";
                    break;
                case ".w":
                    asmcode += $"mov rax,[{lbl}+{24}]";
                    break;
            }
            //removed qword
            asmcode += "push rax";
            code.Put(context, asmcode);
            typeAttr.Put(context, VarType.DOUBLE);
        }
        public override void ExitVardecl(ssuplParser.VardeclContext context)
        {
            // vardecl : TYPE ID
            string vname = context.ID().GetText();
            string typestr = context.TYPE().GetText();
            VarType vtype = stringToType(typestr);
            VarInfo vinfo = new VarInfo(vtype, label(),
                 context.Start.Line);
            symtable.Set(vname, vinfo);
            typeAttr.Put(context, symtable.Get(vname).type);
        }
        public override void ExitAssign1(ssuplParser.Assign1Context context)
        {
            // assign ``:`` ID EQ expr
            string vname = context.ID().GetText();
            if (!symtable.Contains(vname))
                throw new Exception("Undeclared variable: " + vname);
            if (type(context.expr()) != symtable.Get(vname).type)
                throw new Exception("Variable type mismatch in assignment");
            VarType vtype = symtable.Get(vname).type;
            int vsizeInQuads = SizeForType(vtype) / 8;
            string lbl = symtable.Get(vname).location;

            ASM asmcode = new ASM();
            asmcode += code.Get(context.expr());
            for (int i = 0; i < vsizeInQuads; i++)
            {
                asmcode += "pop rax";
                asmcode += $"mov [{lbl}+{i * 8}],rax";
            }
            code.Put(context, asmcode);
            typeAttr.Put(context, vtype);
        }
        public override void ExitAssign2(ssuplParser.Assign2Context context)
        {
            // assign ``:`` ID DOT FIELD EQ expr
            //Console.WriteLine("Exitassign2");
            string vname = context.ID().GetText();
            if (!symtable.Contains(vname))
                throw new Exception("Undeclared variable: " + vname);
            if (type(context.expr()) != VarType.DOUBLE || symtable.Get(vname).type != VarType.VEC4)
                throw new Exception("Variable type mismatch in assignment");
            VarType vtype = symtable.Get(vname).type;
            int vsizeInQuads = SizeForType(vtype) / 8;

            ASM asmcode = new ASM();
            asmcode += code.Get(context.expr());
            asmcode += "pop rax";
            string lbl = symtable.Get(vname).location;
            switch (context.FIELD().GetText())
            {
                case ".x":
                    asmcode += $"mov [{lbl}+{0}], rax";
                    break;
                case ".y":
                    asmcode += $"mov [{lbl}+{8}], rax";
                    break;
                case ".z":
                    asmcode += $"mov [{lbl}+{16}], rax";
                    break;
                case ".w":
                    asmcode += $"mov [{lbl}+{24}], rax";
                    break;
            }
            code.Put(context, asmcode);
            typeAttr.Put(context, vtype);
        }
        public override void ExitCast1(ssuplParser.Cast1Context context)
        {
            var lhstype = stringToType(context.TYPE().GetText());
            var rhstype = type(context.expr());
            typeAttr.Put(context, lhstype);
            if (lhstype == rhstype)
            {
            }
            //casting TO an int FROM a double
            else if (lhstype == VarType.INT && rhstype == VarType.DOUBLE)
            {
                code.Put(context,
                    code.Get(context.expr()),
                    "movq xmm0, [rsp]",      //pop double from stack
                    "add rsp,8",
                    "cvttsd2si rax, xmm0",  //convert double to int
                    "push rax"
                );
            }
            //casting TO a double FROM an int
            else if (lhstype == VarType.DOUBLE && rhstype == VarType.INT)
            {
                code.Put(context,
                    code.Get(context.expr()),
                    "pop rax", //Get the int
                    "cvtsi2sd xmm0, rax",  //convert int to double
                    "sub rsp,8",  //can't use push pop with doubles
                    "movq [rsp], xmm0" //push back on to stack
                );
            }
            else
            {
                throw new Exception("ICE");
            }
        }
    }
    public class StringInfo
    {
        public string address;
        public StringInfo(string addr)
        {
            address = addr;
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

            tokenizer.setInput(idata);
            IList<IToken> tokens = new List<IToken>();
            while (true)
            {
                Token t = tokenizer.next();
                //Console.WriteLine("token: " + t);
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

            //This makes the functions and actual program
            using (var w = new StreamWriter("out.asm"))
            {
                //Console.WriteLine("----------------------------");
                //Console.WriteLine(allcode);
                //Console.WriteLine("----------------------------");
                w.Write(allcode.ToString());
                foreach (var literal in listener.stringPool.Keys)
                {
                    w.WriteLine("; " + literal.Replace("\n", "\\n"));
                    w.WriteLine(listener.stringPool[literal].address + ":");
                    w.Write("db ");
                    byte[] b = Encoding.ASCII.GetBytes(literal);
                    for (int i = 0; i < literal.Length; ++i)
                    {
                        w.Write(b[i]);
                        w.Write(",");
                    }
                    w.WriteLine("0");
                }
            }

            //This makes variables possible in the global data after the program
            //Literally gets append to the bottom of the out.asm file
            var symtable = listener.symtable.table;
            foreach (var sym in symtable)
            {
                //Need dq to have 0,0,0,0 for vec4
                if (sym.Value.type == VarType.VEC4)
                {
                    var globaldata = new ASM(
                        $"{sym.Value.location}:",
                            "dq 0",
                            "dq 0",
                            "dq 0",
                            "dq 0");
                            
                    using (var appendglobals = File.AppendText("out.asm"))
                    {
                        appendglobals.Write(globaldata.ToString());
                    }
                }
                else
                {
                    var globaldata = new ASM(
                        $"{sym.Value.location}:",
                            "dq 0");
                    using (var appendglobals = File.AppendText("out.asm"))
                    {
                        appendglobals.Write(globaldata.ToString());
                    }
                }
            }
        }
    }
}

