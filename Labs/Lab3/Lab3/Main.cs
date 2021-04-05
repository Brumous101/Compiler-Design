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
    static class Annotations
    {
        public static ParseTreeProperty<double> ptp = new ParseTreeProperty<double>();
    }
    public class MyListener : calcBaseListener
    {
        public override void ExitStart(calcParser.StartContext context)
        {
            base.ExitStart(context);
            double v = Annotations.ptp.Get(context.sum());
            Annotations.ptp.Put(context, v);
        }
        public override void ExitSumPlusProduct(calcParser.SumPlusProductContext context)
        {
            base.ExitSumPlusProduct(context);
            var sum = context.sum();
            var product = context.product();
            double v1 = Annotations.ptp.Get(sum);
            double v2 = Annotations.ptp.Get(product);
            double res;
            if (context.ADDOP().GetText() == "+")
                res = v1 + v2;
            else
                res = v1 - v2;
            Annotations.ptp.Put(context, res);
        }
        public override void ExitSumToProduct(calcParser.SumToProductContext context)
        {
            base.ExitSumToProduct(context);
            var product = context.product();
            double v = Annotations.ptp.Get(product);
            Annotations.ptp.Put(context, v);
        }
        public override void ExitProductMultiplyNegate(calcParser.ProductMultiplyNegateContext context)
        {
            base.ExitProductMultiplyNegate(context);
            var sum = context.product();
            var negate = context.negate();
            double v1 = Annotations.ptp.Get(sum);
            double v2 = Annotations.ptp.Get(negate);
            double res;
            if (context.MULOP().GetText() == "*")
                res = v1 * v2;
            else
                res = v1 / v2;
            Annotations.ptp.Put(context, res);
        }
        public override void ExitProductToNegate(calcParser.ProductToNegateContext context)
        {
            base.ExitProductToNegate(context);
            var negate = context.negate();
            double v = Annotations.ptp.Get(negate);
            Annotations.ptp.Put(context, v);
        }
        public override void ExitFactorToParens(calcParser.FactorToParensContext context)
        {
            base.ExitFactorToParens(context);
            var parens = context.parens();
            double v = Annotations.ptp.Get(parens);
            Annotations.ptp.Put(context, v);
        }
        public override void ExitFactorToNum(calcParser.FactorToNumContext context)
        {
            base.ExitFactorToNum(context);
            double value = Double.Parse(context.NUM().GetText());
            Annotations.ptp.Put(context, value);
        }
        public override void ExitParens(calcParser.ParensContext context)
        {
            base.ExitParens(context);
            var sum = context.sum();
            double v = Annotations.ptp.Get(sum);
            Annotations.ptp.Put(context, v);
        }
        public override void ExitAddopNegate(calcParser.AddopNegateContext context)
        {
            base.ExitAddopNegate(context);
            var negate = context.negate();
            double v2 = Annotations.ptp.Get(negate);
            double res = v2 * (-1);
            Annotations.ptp.Put(context, res);
        }
        public override void ExitNegateToPower(calcParser.NegateToPowerContext context)
        {
            base.ExitNegateToPower(context);
            var power = context.power();
            double v = Annotations.ptp.Get(power);
            Annotations.ptp.Put(context, v);
        }
        public override void ExitFactorPowopNegate(calcParser.FactorPowopNegateContext context)
        {
            base.ExitFactorPowopNegate(context);
            var factor = context.factor();
            var negate = context.negate();
            double v1 = Annotations.ptp.Get(factor);
            double v2 = Annotations.ptp.Get(negate);
            double res = Math.Pow(v1, v2);
            Annotations.ptp.Put(context, res);
        }
        public override void ExitPowerToFactor(calcParser.PowerToFactorContext context)
        {
            base.ExitPowerToFactor(context);
            var factor = context.factor();
            double v = Annotations.ptp.Get(factor);
            Annotations.ptp.Put(context, v);
        }
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
        }
    }
}

