using System;
namespace Interpreter;


class Massa {
    public static void Main(string[] args) {
        Console.Write(">>> ");
         Lexer l = new Lexer(Console.ReadLine());
         Parser p = new Parser(l);
	 Interpreter i = new Interpreter(p);

	 Console.WriteLine(i.Interpret());
    }
}
