
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
            Tokenizer T = new Tokenizer(readFile("../../../terminals.txt"));
            
            string input = Console.ReadLine();

            ShuntingYard N = new ShuntingYard(T, input);
        }
    }
}

