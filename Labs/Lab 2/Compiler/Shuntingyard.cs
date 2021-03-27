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
        public TreeNode(string sym, string token)
        {
            this.sym = sym;
            this.token = token;
        }
        public void addChild(string name, Token tok)
        {
            this.children.Add(new TreeNode(name, tok));
        }
    }
    public class ShuntingYard
    {
        public Stack<TreeNode> numStack = new Stack<TreeNode>();
        public Stack<TreeNode> operatorStack = new Stack<TreeNode>();

        public ShuntingYard(Tokenizer T)
        {
            while (T.atEnd())
            {
                Token t = T.next();
                if ((t.Symbol == "NUM") || (t.Symbol == "ID"))
                    numStack.Push(new TreeNode(t.Symbol, t.Lexeme));
                else
                    handleOperator(t, operatorStack, numStack);
            }
        }
        public void addNode(string name, Token tok)
        {
            this.children.Add(new TreeNode(name, tok));
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
            {
                Console.WriteLine("Symbol for precedence not defined");
                return -1;
            }
        }
        public void handleOperator(Token t, Stack<TreeNode> operatorStack, Stack<TreeNode> numStack)
        {
            while (true)
            {
                if (operatorStack.Count == 0)
                    break;
                var A = operatorStack.Peek();
                if (precedence(A) >= precedence(t.Symbol))
                {
                    var opNode = operatorStack.Pop();
                    var c1 = numStack.Pop();
                    var c2 = numStack.Pop();
                // notice order: First popped = last added
                    opNode.addChild(c2)
                    opNode.addChild(c1)
                    numStack.push(opNode)
                }
                else
                    break;
                operatorStack.push(Node(t.sym, t.lexeme))
            }
        }
    }
}
