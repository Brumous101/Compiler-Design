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
                //Console.WriteLine("My token symbol:" + t.Symbol);
                //Console.WriteLine("My token Lexeme:" + t.Lexeme);
                if (t.Lexeme == "-")
                {
                    if ( (previous == null) || (previous.Symbol == "LPAREN") || (previous.Symbol == "NUM") || (previous.Symbol == "ID"))
                    {
                        t.Symbol = "NEGATE";
                    }
                }
                if ((t.Symbol == "NUM") || (t.Symbol == "ID")) //ID could be variable num is just numbers
                    numStack.Push(new TreeNode(t.Symbol, t));
                /*else if (t.Symbol == "WHITESPACE")
                {
                    Console.WriteLine("WHit");
                }*/
                else
                    handleOperator(t, operatorStack, numStack);
            }

            while (operatorStack.Count() != 0 ) //while not empty
            {
                var opNode = operatorStack.Pop();
                var c1 = numStack.Pop();
                var c2 = numStack.Pop();
                opNode.addChild(c2);
                opNode.addChild(c1);
                //If we got something on the operator stack, +-*, at this point we have an operator that needs dealt with
                //opNode is +, get it's two children, two most recent. Pop those c1 and c2. c2, this is like creating the leaf nodes
                //the thing on numstack that gets pushed is like
                //     +
                //    /  \        <--opNode
                //   1    2     
                numStack.Push(opNode);
                // Console.WriteLine("numStack Peek" + numStack.Peek().sym);
                // Parse through these nodes and solves
                // if sym opNode
                // At this point we want to check to
                // sym == "ADDOP"
                // c2 + c1
                // 
                // COME BACK TO THIS
            }
            // nothing left in operatorStack
            // ALL NODES ARE IN NUMSTACK!
            //Console.WriteLine(numStack.Peek().sym);

            //recursively goes
            var numNode = numStack.Pop();
            var answer = handleNode(numNode);
            Console.WriteLine(answer);

            // if you have an op on numstack it has children 
            // to reference pop
            //Console.WriteLine(handleNode(numStack.Peek().children[0]));
            //Console.WriteLine(handleNode(numStack.Peek().children[1]));
        }
        // tree of nodes
        public Double handleNode(TreeNode node)
        {
            //Double num1 = Double.Parse(node.children[0].token.Lexeme);
            // Double num2 = Double.Parse(node.children[1].token.Lexeme);

            if (node.sym == "NUM")
                return Double.Parse(node.token.Lexeme);
            else if (node.sym == "ADDOP")
            {
                var v1 = handleNode(node.children[0]);
                var v2 = handleNode(node.children[1]);
                return v1 + v2;
            }
            else if (node.sym == "MULOP")
            {
                var v1 = handleNode(node.children[0]);
                var v2 = handleNode(node.children[1]);
                return v1 * v2;
            }
            else if (node.sym == "POWOP")
            {
                var v1 = handleNode(node.children[0]);
                var v2 = handleNode(node.children[1]);
                return Math.Pow(v1, v2);
            }
            else
            {
                throw new InvalidOperationException("Operator not found!!!");
            }
        }
        public static int precedence(string sym)//return how high of a operator we have
        {
            if (sym == "POWOP")
                return 10;
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
                if (operatorStack.Count == 0)
                    break;
                var A = operatorStack.Peek().sym;//Peek and top do the same thing I think
                if ( (assoc =="L" && (precedence(A) >= precedence(t.Symbol) )) || ( assoc == "R" && (precedence(A) > precedence(t.Symbol)) ) )
                {
                    var opNode = operatorStack.Pop();
                    var c1 = numStack.Pop();
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
