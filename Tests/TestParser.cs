using System;
using Xunit;
using Interpreter;

namespace Tests;

public class TestParser
{
    public AST Prepare(string text) {
        Lexer lexer = new Lexer(text);
        Parser parser = new Parser(lexer);

        return parser.Parse();
    }

    public Interpreter.Interpreter PrepareInterpreter(string text) {
        Lexer lexer = new Lexer(text);
        Parser parser = new Parser(lexer);

        return new Interpreter.Interpreter(parser);
    }

    public dynamic Interpret(string text) {
        return this.PrepareInterpreter(text).Interpret();
    }



    public static void AssertEqual(dynamic a, AST b, Context ctx = null) {
        Assert.Equal(a, b.Eval(ctx));
    }

    [Fact]
    public void TestBasicTypeChecking() {
        var result = (BlockNode) this.Prepare("5 + 5;");
        Assert.Equal(AST<object>.FLOAT, result.blocks[0].Type);
    }

    [Fact]
    public void TestBuiltinTypeChecking() {
        // we need an interpreter to evaluate the function to infer the type
        // runtime type checking
        var result = this.Interpret("log(2) + 5;");
        Assert.Equal((float) Math.Log(2) + 5, result);
    }

    [Fact]
    public void TestTypeError() {
        // we need an interpreter to evaluate the function to infer the type
        // runtime type checking
        Assert.Throws<TypeError>(delegate {this.Interpret("5 + \"a\";");});
    }

    [Fact]
    public void TestVarTypeCheck() {
        // we need an interpreter to evaluate the function to infer the type
        // runtime type checking
        Assert.Throws<TypeError>(delegate {this.Interpret("a =1;a+\"\";");});
    }
}
