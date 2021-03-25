
using System;
using System.IO;

namespace parser
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
            Tokenizer T = null;
            string input = "";
            //string terminals = readFile("terminals.txt");
            //Console.WriteLine("-------------->");
            //Console.WriteLine(terminals);
            //Console.WriteLine("<--------------");
            input = Console.ReadLine();
            Console.WriteLine("Does grammar contains? "+input.IndexOf("1"));
            Console.WriteLine("Printing out the typed input: " + input);
            //Console.WriteLine(grammar);
            //Console.WriteLine(T = new Tokenizer(readFile("terminals.txt")));
            T = new Tokenizer(readFile("terminals.txt"));
            //Console.WriteLine("Creating tokenizer for " + terminals + "...");
            T.setInput(input);/*
            using (var fs = new StreamReader("terminals.txt"))
            {
                while (true)
                {
                    string line = fs.ReadLine();
                    if (line == null)
                        break;
                    var lst = line.Split(' ');
                    //string terminalSymbol = lst[0];
                    //string terminalRegex = lst[2];
                    //string inpspec = Path.Combine("tests", lst[1]);
                    //string expspec = Path.Combine("tests", lst[2]);
                    Console.WriteLine("terminalSymbol: " + terminalSymbol + "terminalRegex: " + terminalRegex);
                }
            }*/
            while (true)
            {
                Console.WriteLine("---------------->");
                var tok = T.next();
                Console.WriteLine(tok);
                Console.WriteLine("<----------------");
                if (tok.Symbol == "$")
                {
                    Console.WriteLine("Reached EOF");
                    break;
                }
                Console.WriteLine("Input:\n" + input);
                Console.WriteLine("\tGot:" + tok);
                return;
            }
        }
    }
}

