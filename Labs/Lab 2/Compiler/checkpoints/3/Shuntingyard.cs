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
        public TreeNode(string sym, Token token)
        {
            this.sym = sym;
            this.token = token;
        }
    }
    public class ShuntingYard
    {
        public List<TreeNode> children = new List<TreeNode>();
        public Stack<int> numStack = new Stack<int>();
        public Stack<int> operatorStack = new Stack<int>();

        public ShuntingYard()
        {
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
    }
}
