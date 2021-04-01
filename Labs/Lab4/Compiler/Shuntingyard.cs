using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class TreeNode
    {
        public string sym;
        public Token token;
        public List<TreeNode> children = new List<TreeNode>();
        public TreeNode(string sym, Token token)
        {
            this.sym = sym;
            this.token = token;
        }
        public void addChild(TreeNode node)
        {
            children.Add(node);

            //Console.WriteLine("Child:" + children[0]);
        }
    }
    public class ShuntingYard
    {
        public Stack<TreeNode> numStack = new Stack<TreeNode>();
        public Stack<TreeNode> operatorStack = new Stack<TreeNode>();

        public ShuntingYard(Tokenizer T,string input)
        {
            T.setInput(input);
            Token previous = null;
            Token t = null;
            while ( !(T.atEnd()) )
            {
                previous = t;
                t = T.next();
                if (t.Lexeme == "-")
                {
                    if ( (previous == null) || (previous.Symbol == "LPAREN")  || ( (previous.Symbol != "NUM") && (previous.Symbol != "ID") && (previous.Symbol != "RPAREN")) )
                    {
                        t.Symbol = "NEGATE";
                    }
                }
                if ((t.Symbol == "NUM") || (t.Symbol == "ID")) //ID could be variable num is just numbers
                    numStack.Push(new TreeNode(t.Symbol, t));
                else if (t.Symbol == "LPAREN")
                {
                    operatorStack.Push(new TreeNode(t.Symbol, t));
                }
                else if (t.Symbol == "RPAREN")
                {
                    //Console.WriteLine("HERE");
                    while (operatorStack.Peek().sym.Contains("LPAREN") == false)
                    {
                        var opNode = operatorStack.Pop();
                        var c1 = numStack.Pop();
                        if (opNode.sym != "NEGATE")
                        {
                            var c2 = numStack.Pop();
                            opNode.addChild(c2);
                        }
                        opNode.addChild(c1);
                        numStack.Push(opNode);
                    }
                    operatorStack.Pop();
                }
                else
                    handleOperator(t, operatorStack, numStack);
            }

            while (operatorStack.Count() != 0 ) //while not empty m
            {
                var opNode = operatorStack.Pop();
                var c1 = numStack.Pop();
                if (opNode.sym != "NEGATE")
                {
                    var c2 = numStack.Pop();
                    opNode.addChild(c2);
                }
                opNode.addChild(c1);
                numStack.Push(opNode);
            }
            var numNode = numStack.Pop();
            var answer = handleNode(numNode);
            Console.WriteLine(answer);
        }
        // tree of nodes
        public Double handleNode(TreeNode node)
        {
            if (node.sym == "NUM")
                return Double.Parse(node.token.Lexeme);
            var v1 = handleNode(node.children[0]);
            if (node.sym == "NEGATE")
                return -v1;
            var v2 = handleNode(node.children[1]);

            switch (node.token.Lexeme)
            {
                case "+":
                    return v1 + v2;

                case "-":
                    return v1 - v2;

                case "*":
                    return v1 * v2;

                case "/":
                    return v1 / v2;

                case "**":
                    return Math.Pow(v1, v2);

                default:
                    throw new InvalidOperationException("Operator not found!!!");
            }

        }
        public static int precedence(string sym)//return how high of a operator we have
        {
            if (sym == "POWOP")
                return 10;
            else if (sym == "NEGATE")
                return 9;
            else if (sym == "MULOP")
                return 5;
            else if (sym == "ADDOP")
                return 1;
            else
                throw new InvalidOperationException("Operator in precedence function not defined");
        }
        public void handleOperator(Token t, Stack<TreeNode> operatorStack, Stack<TreeNode> numStack)
        {
            // ** has to be right associative and not left
            // 3**2**2
            // 3**4
            // NOT 9**2
            string assoc = associativity(t.Symbol);
            string sym = t.Symbol;
            while (true)
            {
                if (operatorStack.Count == 0 || operatorStack.Peek().sym == "LPAREN")
                    break;
                var A = operatorStack.Peek().sym;//Peek and top do the same thing I think
                if (t.Symbol == "NEGATE" && (A == "POWOP"))
                    break;
                else if ((assoc == "L" && (precedence(A) >= precedence(t.Symbol))) || (assoc == "R" && (precedence(A) > precedence(t.Symbol))))
                {
                    var opNode = operatorStack.Pop();
                    var c1 = numStack.Pop();
                    //Console.WriteLine("c1"+c1.sym);
                    if (arity(A) == 2)// check the top thing in op stack's arity
                    {
                        var c2 = numStack.Pop();
                        // notice order: First popped = last added
                        opNode.addChild(c2);
                    }
                    opNode.addChild(c1);
                    numStack.Push(opNode);
                }
                else
                    break;
            }
            operatorStack.Push(new TreeNode(t.Symbol, t));
        }
        public string associativity(string sym)
        {
            // ** has to be right associative and not left
            // 3**2**2
            // 3**4
            // NOT 9**2
            if (sym == "POWOP" || sym == "NEGATE")
                return "R";
            else
                return "L";
        }
        public int arity(string nary)
        {
            // arity is necessary cause if you try to look at the second object and it's arity is 1 you will look... annoying
            // if its a negate you will have a number joined by just the negate sym so it will be 1
            // if its a ADDOP you will have two numbers joined by a ADDOP so it will be 2
            if( nary == "NEGATE")
                return 1;
            else
                return 2;
        }
    }
}
