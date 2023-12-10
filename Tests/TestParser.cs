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

    [Fact]
    public void TestRecursiveTypeCheck() {
        Assert.Throws<TypeError>(delegate {this.Prepare(@"
        f(n, k) = if count(n) > -k
                then
                  let a, rest = n;
                      in f(rest)
                else
                  let a, rest = n;
                      in a;
        -f({point(0,0), point(1,0)}, -1);
        ");});
    }

    [Fact]
    public void TestCheckFnPropagation() {
        Assert.Throws<TypeError>(delegate {this.Prepare("f(n) = 1+n;f(\"blob\");");});
    }

    [Fact]
    public void TestRecFnPropagation() {
        Assert.Throws<TypeError>(delegate {this.Prepare("f(n) = if (n<1) then n else f(n-1);f(\"blob\");");});

    }

    [Fact]
    public void TestIdentity() {
        var result = (BlockNode) this.Prepare("i(n)=n;i(1);i(\"2\");i(point(0,0));");
        // assert it doesn't throw an error
    }

    [Fact]
    public void TestIdentityOps() {
        var result = (BlockNode) this.Prepare("i(n)=n;i(1);i({1,2,3})+{4,5};");
        Assert.Throws<TypeError>(delegate {this.Prepare("i(n)=n;i(1);i({1,2,3})+{\"three\",\"four\"};");});
    }
}
