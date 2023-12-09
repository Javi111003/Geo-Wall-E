using System;
using Xunit;
using Interpreter;

namespace Tests;

public class TestParser
{
    public AST Prepare(string text) {
        Lexer lexer = new Lexer(text, debug:true);
        Parser parser = new Parser(lexer, debug:true);

        return parser.Parse();
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
    public void TestLetinType() {
        var result = (BlockNode) this.Prepare("let a = 5; in a;");
        Assert.Equal(AST<object>.INTEGER, result.blocks[0].Type);
    }

    [Fact]
    public void TestLetinTypeError() {
        Assert.Throws<TypeError>(delegate {this.Prepare("let a = 5; in a + \"blob\";");});
    }

    [Fact]
    public void TestLetinAsExpr() {
        var result = (BlockNode) this.Prepare("1 + (let x = 2; in x * x);");
        Assert.Equal(AST<object>.FLOAT, result.blocks[0].Type);
    }

    [Fact]
    public void TestInfSeq() {
        var result = (BlockNode) this.Prepare("a,b,c = {1...};c;b;");
        return;
        Assert.Equal(AST<object>.SEQUENCE, result.blocks[1].Type);
        Assert.Equal(AST<object>.INTEGER, result.blocks[2].Type);
    }

    [Fact]
    public void TestConditionalTypeCheck() {
        Assert.Throws<TypeError>(delegate {this.Prepare("if 1 then \"blob\" else 1;");});
    }

    [Fact]
    public void TestConditionalBodyTypeCheck() {
        Assert.Throws<TypeError>(delegate {this.Prepare("if 1+\"\" then 1 else 1;");});
        Assert.Throws<TypeError>(delegate {this.Prepare("if 1 then 1+\"\" else 1;");});
        Assert.Throws<TypeError>(delegate {this.Prepare("if 1 then 1 else 1+\"\";");});
    }

    [Fact]
    public void TestVariableDeclTypeCheck() {
        Assert.Throws<TypeError>(delegate {this.Prepare("a = \"\"+1;");});
    }

    [Fact]
    public void TestSequenceTypeCheck() {
        Assert.Throws<TypeError>(delegate {this.Prepare("{\"\", 1};");});
    }

    [Fact]
    public void TestFunctionTypeCheck() {
        Assert.Throws<TypeError>(delegate {this.Prepare("a() = 1 + \"\";");});
    }

    [Fact]
    public void TestUndefined() {
        var result = (BlockNode) this.Prepare("undefined;");
        return;
        Assert.Equal(AST<object>.SEQUENCE, result.blocks[0].Type);
    }

    [Fact]
    public void TestBuiltinsTypes() {
        // long-awaited test
        Assert.Throws<TypeError>(delegate {this.Prepare("log(2) + \"\";");});
    }

    [Fact]
    public void TestOperatorCheckPropagation() {
        Assert.Throws<TypeError>(delegate {this.Prepare("1 + \"\" + 1;");});
    }
    public void TestOperatorCheckPropagation2() {
        Assert.Throws<TypeError>(delegate {this.Prepare("1 + 1 + \"\";");});
    }

    public void TestOperatorCheckPropagation3() {
        Assert.Throws<TypeError>(delegate {this.Prepare("-\"\";");});
    }
}
