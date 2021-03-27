
using System;
using System.IO;

namespace Compiler
{

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
            Tokenizer T = new Tokenizer(readFile("../../terminals.txt"));
            ShuntingYard N = new ShuntingYard();
            string input = "";
            int i = 1;
            input = Console.ReadLine();
            Console.WriteLine("Printing out the typed input: " + input);
            T.setInput(input);
            var tok = T.next();
            
            while (tok.Symbol != "$")
            {
                //i need to turn entry into precedence
                i = ShuntingYard.precedence(tok.Symbol);
                string precedence = i.ToString();
                N.addNode(precedence, tok);
                if (tok.Symbol == "$")
                {
                    Console.WriteLine("Reached EOF");
                    break;
                }
                tok = T.next();
            }
            Console.WriteLine("Printing Nodes:");
            foreach (TreeNode node in N.children)
            {
                Console.WriteLine(" "+node.sym+" tok.sym " + node.token.Symbol);
            }
        }
    }
}

