using System;

using Interpreter;

class Massa {
    static string msg = "H.U.L.K. REPL\nInterpreter version: 0.0.0\nREPL version: 0.0.0\n";
    
    public static void Main(string[] args) {
	Lexer l = null;
	Parser p = null;
	Interpreter.Interpreter i = null;
	try {
	    Console.Write(msg);
	    l = new Lexer(";");
	    p = new Parser(l);
	    i = new Interpreter.Interpreter(p);
	}
	catch (Exception e) {
	    Console.WriteLine(e);
	    return;
	}
	
        while (true) {
            Console.Write(">>> ");
            
            try {
                l = new Lexer(Console.ReadLine());
                p = new Parser(l);
                i.parser = p;
                Console.WriteLine(i.Interpret());
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}
