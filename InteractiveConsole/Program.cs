using System;

using Interpreter;

class Master {
    static string msg = "H.U.L.K. REPL\nInterpreter version: 0.0.0\nREPL version: 0.0.0\n";
    
    [STAThread]
    public static void Main(string[] args) {
	Lexer lexer = null;
	Parser parser = null;
	_Interpreter interpreter = null;
	try {
	    Console.Write(msg);
	    lexer = new Lexer(";");
	    parser = new Parser(lexer);
	    interpreter = new _Interpreter(parser);
	}
	catch (Exception e) {
	    Console.WriteLine(e);
	    return;
	}
    Context context;
    while (true) {
        Console.Write(">>> ");
        context = parser.local_context;
        try {
            lexer = new Lexer(Console.ReadLine());
            parser = new Parser(lexer, context);
            interpreter.parser = parser;
            Console.WriteLine(interpreter.Interpret());
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }
    }
    }
}
