using System;
using Xunit;
using Interpreter;

namespace Tests;

public class TestIntegration
{
    public Interpreter.Interpreter _Prepare(string text) {
        Lexer lexer = new Lexer(text);
        Parser parser = new Parser(lexer);

        return new Interpreter.Interpreter(parser);
    }

    public dynamic _Interpret(string text) {
        return this._Prepare(text).Interpret();
    }

    [Fact]
    public void TestLiteral() {
        var result = this._Interpret("5;");
        Assert.Equal(5, result);

        result = this._Interpret("\"blob\";");
        Assert.Equal("blob", result);

        result = this._Interpret("7.03;");
        Assert.Equal(7.03, Math.Round(result, 3));

        result = this._Interpret("True;");
        Assert.Equal(true, result);
    }

    [Fact]
    public void TestUnary() {
        var result = this._Interpret("-5;");
        Assert.Equal(-5, result);

        result = this._Interpret("--(1 + 1);");
        Assert.Equal(2, result);

        result = this._Interpret("!1;");
        Assert.Equal(false, result);

        result = this._Interpret("a = !1;!a;");
        Assert.Equal(true, result);
    }

    [Fact]
    public void TestBasicOperation() {
        var result = this._Interpret("5 + 10;");
        Assert.Equal(15, result);

        result = this._Interpret("5 + 10.1;");
        Assert.Equal(15.1, Math.Round(result, 3));

        result = this._Interpret("5 - 10;");
        Assert.Equal(-5, result);

        result = this._Interpret("10 / 5;");
        Assert.Equal(2, result);

        result = this._Interpret("5 % 10;");
        Assert.Equal(5, result);

        result = this._Interpret("5 * 10;");
        Assert.Equal(50, result);

        result = this._Interpret("5 ^ 2;");
        Assert.Equal(25, result);
    }

    [Fact]
    public void TestComplexOperation() {
        var result = this._Interpret("((5 * 2) + 10);");
        Assert.Equal(20, result);

        result = this._Interpret("(5 ^ 2) + (10 / 2);");
        Assert.Equal(30, result);
    }

    [Fact]
    public void TestBuiltins() {
        var result = this._Interpret("log(2);");
        Assert.Equal((float) System.Math.Log(2), result);
    }

    [Fact]
    public void TestAssignment() {
        var result = this._Interpret("a = 5; a;");
        Assert.Equal(5, result);
    }

    [Fact]
    public void TestLambda() {
        var result = this._Interpret("7 + (let x = 2 in x * x);");
        Assert.Equal(11, result);

        result = this._Interpret("\"blob\";");
        Assert.Equal("blob", result);

        result = this._Interpret("7.03;");
        Assert.Equal(7.03, Math.Round(result, 3));
    }

    [Fact]
    public void TestFinline() {
        var result = this._Interpret("blob(x) = x*x; blob(5);");
        Assert.Equal(25, result);
    }

    [Fact]
    public void TestConditional() {
        var result = this._Interpret("if (0) \"blob\" else \"doko\";");
        Assert.Equal("doko", result);
    }

    [Fact]
    public void TestBoolean() {
        var result = this._Interpret("if (1 > 0) \"blob\" else \"doko\";");
        Assert.Equal("blob", result);

        result = this._Interpret("if (1 == 0) \"blob\" else \"doko\";");
        Assert.Equal("doko", result);
    }

    [Fact]
    public void TestRecursive() {
        var result = this._Interpret("fib(n) = if (n > 1) fib(n-1) + fib(n-2) else 1;(fib(5));");
        Assert.Equal(8, result);
    }

    [Fact]
    public void TestFnEmptyArgs() {
        var result = this._Interpret("blob() = \"doko\"; blob();");
        Assert.Equal("doko", result);
    }

    [Fact]
    public void TestLocalContext() {
        var result = this._Interpret("x() = 5; blob(x) = x;blob(3);");
        Assert.Equal(3, result);
        result = this._Interpret("x = 5; blob() = x;blob();");
        Assert.Equal(5, result);
    }

    [Fact]
    public void TestMemeEdgeCase() {
        var result = this._Interpret("-5 - 3;");
        Assert.Equal(-8, result);
        result = this._Interpret("-5 - -3;");
        Assert.Equal(-2, result);
    }


    [Fact]
    public void TestTypedMultiDecl() {
        // [function] [name] == [function] = [type]();
        var result = this._Interpret("five() = 5;five p1;p1;");
        Assert.Equal(5, result);
    }
}
