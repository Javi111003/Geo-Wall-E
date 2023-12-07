using System;
namespace Interpreter;


class Massa {
    public static void Main(string[] args) {
         Lexer l = new Lexer(";");
         l = new Lexer(l.TextFrom(args[0]));
         Parser p = new Parser(l);
         Interpreter i = new Interpreter(p);

         Console.WriteLine(i.Interpret());
    }
}
