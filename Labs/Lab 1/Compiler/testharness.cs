
using System;
using System.IO;

namespace parser{

    class MainClass{
        
        static string readFile(string f){
            using(var fs = new StreamReader(f)){
                return fs.ReadToEnd();
            }
        }
        
        public static void Main(string[] args){
            
            string prevGrammar = "";
            
            using(var fs = new StreamReader(Path.Combine("tests","tests.txt"))){
                Tokenizer T=null;
                int i=1;
                
                while (true){
                    string line = fs.ReadLine();
                    if( line == null )
                        break;
                    var lst = line.Split(' ');
                    string gspec = Path.Combine("tests",lst[0]);
                    string inpspec = Path.Combine("tests",lst[1]);
                    string expspec = Path.Combine("tests",lst[2]);
                    //Console.WriteLine("------------------>" + readFile(gspec));

                    //Console.WriteLine("lst[0]: "+lst[0]+" gspec: "+gspec);

                    //Console.WriteLine("Test "+i);
                    i++;
                    
                    if( gspec != prevGrammar ){
                        prevGrammar=gspec;
                        T = new Tokenizer(readFile(gspec));
                        //Console.WriteLine("yo here    :" + readFile(gspec));
                        Console.WriteLine("Creating tokenizer for "+gspec+"...");
                    }
                    else{
                        Console.WriteLine("Reusing tokenizer...");
                    }
        
                    //Console.WriteLine("Input "+inpspec);
                    //Console.WriteLine("------------------>" + readFile(inpspec) + "<---------------");
                    T.setInput(readFile(inpspec));//sets to the content of file
            
                    var expected = readFile(expspec).Split('\n');
                    int j=0;
                    while(true){
                        string expsym = expected[j++];
                        int expline = (expected[j] == "ERROR" ? -1: Int32.Parse(expected[j]) );
                        j++;
                        string explexeme = expected[j++];
                        try{
                            var tok = T.next();
                            Console.WriteLine(tok);

                            if ( expsym == "ERROR" ){
                                Console.WriteLine("Did not expect to get token here\n");
                                return;
                            }
                            
                            if( expsym == "$" && tok.Symbol == "$" ){
                                break;
                            }
                            
                            if( expsym != tok.Symbol || explexeme != tok.Lexeme || expline != tok.Line ){
                                Console.WriteLine("\n\nMismatch!\n");
                                Console.WriteLine("Input:\n"+inpspec);
                                Console.WriteLine("\tGot:"+tok);
                                Console.WriteLine("\tExpected:");
                                Console.WriteLine("\t\tSymbol:"+expsym);
                                Console.WriteLine("\t\tLine:"+expline);
                                Console.WriteLine("\t\tLexeme:"+explexeme);
                                return;
                            }
                            Console.WriteLine("Passed as it should have. Good.");
                        } catch(Exception e){
                            if( expsym == "ERROR"){
                                //failure was expected
                                Console.WriteLine("Failed as it should have. Good.");
                                break;
                            } else {
                                throw(e);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("All tests OK\n");
        }
    }
}

