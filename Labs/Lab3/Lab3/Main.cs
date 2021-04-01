using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

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
    public class MyListener : calcBaseListener
    {
        public override void ExitFactor(calcParser.FactorContext context)
        {
            base.ExitFactor(context);
            double value = Double.Parse(context.NUM().GetText());
            Annotations.ptp.Put(context, value);
        }
        public override void ExitStart(calcParser.StartContext context)
        {
            base.ExitStart(context);
            double v = Annotations.ptp.Get(context.sum());
            Annotations.ptp.Put(context, v);
        }
        public override void ExitSumPlusFactor(calcParser.SumPlusFactorContext context)
        {
            base.ExitSumPlusFactor(context);
            var sum = context.sum();
            var factor = context.factor();
            double v1 = Annotations.ptp.Get(sum);
            double v2 = Annotations.ptp.Get(factor);
            double res;
            if (context.ADDOP().GetText() == "+")
                res = v1 + v2;
            else
                res = v1 - v2;
            Annotations.ptp.Put(context, res);
        }
        public override void ExitSumToFactor(calcParser.SumToFactorContext context)
        {
            base.ExitSumToFactor(context);
            var factor = context.factor();
            double v = Annotations.ptp.Get(factor);
            Annotations.ptp.Put(context, v);
        }
    }
    static class Annotations
    {
        public static ParseTreeProperty<double> ptp = new ParseTreeProperty<double>();
    }

    public class MainClass
    {
        public static void Main(string[] args)
        {
            Syms.init("calc.tokens");
            string gdata;
            using (var r = new StreamReader("terminals.txt"))
            {
                gdata = r.ReadToEnd();
            }
            var tokenizer = new Tokenizer(gdata);
            string idata = Console.ReadLine();
            //var input = Console.ReadLine();
             
            //using (var r = new StreamReader("input.txt"))
            //{
            //    idata = r.ReadToEnd();
            //}
            
            tokenizer.setInput(idata);
            IList<IToken> tokens = new List<IToken>();
            while (true)
            {
                Token t = tokenizer.next();
                if (t.Symbol == "$")
                    break;  //at end
                            //CommonToken is defined in the ANTLR runtime
                CommonToken T = new CommonToken(Syms.stringToInt[t.Symbol], t.Lexeme);
                T.Line = t.Line;
                tokens.Add(T);
            }
            var antlrtokenizer = new BufferedTokenStream(new ListTokenSource(tokens));
            var parser = new calcParser(antlrtokenizer);
            parser.BuildParseTree = true;
            //optional: parser.ErrorHandler = new BailErrorStrategy ();
            //'start' should be the name of the grammar's start symbol
            var listener = new MyListener();
            var walker = new ParseTreeWalker();
            var antlrroot = parser.start();
            walker.Walk(listener, antlrroot);
            double v = Annotations.ptp.Get(antlrroot);
            Console.WriteLine($"{v}");

            //Regex re = new Regex("\n[ \t]+([^\n]+)");
            //string input = "";
            //re.Replace(input, " $1");
        }
    }
}
