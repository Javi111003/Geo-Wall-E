using System;
using System.IO;
using System.Reflection;

using Xunit;

using Interpreter;

namespace Tests;

public class TestIntegration
{
    static public DirectoryInfo BASE_DIR = new DirectoryInfo(
        Assembly.GetAssembly(typeof (TestIntegration)).Location
    ).Parent.Parent.Parent.Parent;

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
        var result = this._Interpret("7 + (let x = 2; in x * x);");
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
        var result = this._Interpret("if 0 then \"blob\" else \"doko\";");
        Assert.Equal("doko", result);
    }

    [Fact]
    public void TestBoolean() {
        var result = this._Interpret("if 1 > 0 then \"blob\" else \"doko\";");
        Assert.Equal("blob", result);

        result = this._Interpret("if 1 == 0 then \"blob\" else \"doko\";");
        Assert.Equal("doko", result);
    }

    [Fact]
    public void TestRecursive() {
        var result = this._Interpret("fib(n) = if n > 1 then fib(n-1) + fib(n-2) else 1;(fib(5));");
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

    [Fact]
    public void TestInvalidLetin() {
        // [function] [name] == [function] = [type]();
        Assert.Throws<NameError>(
                delegate {
                this._Interpret(
                        "let a = (let b = 4 + a; in b); in a;"
                );}
       );
    }

    [Fact]
    public void TestSequenceBasic() {
        // [function] [name] == [function] = [type]();
        var result = this._Interpret("{1,2,3};");
        // to string
        // this is a sequence literal
        Assert.Equal("{1, 2, 3, }", result.ToString());
    }

    [Fact]
    public void TestSequence() {
        // [function] [name] == [function] = [type]();
        var result = this._Interpret("a = {1,2,3};a;");
        // to string
        // this is a sequence literal
        Assert.Equal("{1, 2, 3, }", result.ToString());
    }

    [Fact]
    public void TestSequenceComplex() {
        // [function] [name] == [function] = [type]();
        var result = this._Interpret("a, rest = {1,2,3};a;");
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestSequenceWithRest() {
        // [function] [name] == [function] = [type]();
        var result = this._Interpret("a, rest = {1,2,3};rest;");
        Assert.Equal("{2, 3, }", result.ToString());
    }

    [Fact]
    public void TestCommento() {
        // [function] [name] == [function] = [type]();
        var file = new FileInfo(Path.Join(BASE_DIR.ToString(), "Sample.geo"));
        string code = File.ReadAllText(file.ToString());
        var result = this._Interpret(code);
        Assert.Equal(0, result);
    }

    [Fact]
    public void TestLetinSequenceCombo() {
        // [function] [name] == [function] = [type]();
        var result = this._Interpret("let a, rest = {1,2,3}; b = 5; in a;");
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestBuiltinTypeChecking() {
        // we need an interpreter to evaluate the function to infer the type
        // runtime type checking
        var result = this._Interpret("log(2) + 5;");
        Assert.Equal((float) Math.Log(2) + 5, result);
    }

    [Fact]
    public void TestTypeError() {
        // we need an interpreter to evaluate the function to infer the type
        // runtime type checking
        Assert.Throws<TypeError>(delegate {this._Interpret("5 + \"a\";");});
    }

    [Fact]
    public void TestVarTypeCheck() {
        // we need an interpreter to evaluate the function to infer the type
        // runtime type checking
        Assert.Throws<TypeError>(delegate {this._Interpret("a =1;a+\"\";");});
    }
}
