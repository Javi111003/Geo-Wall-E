using Xunit;
using Interpreter;

namespace Tests;

public class TestLexer
{
    [Fact]
    public void TestInteger() {
        Lexer l = new Lexer("le 5;");
        l.GetNextToken();
        Assert.Equal(l.GetNextToken(), new Token(Tokens.INTEGER, "5"));
    }

    [Fact]
    public void TestString() {
        Lexer l = new Lexer("blob doko \"lorem\" noger;");
        l.GetNextToken();
        l.GetNextToken();
        Assert.Equal(l.GetNextToken(), new Token(Tokens.STRING, "lorem"));
    }

    [Fact]
    public void TestBadString() {
        Lexer l = new Lexer("blob doko \"lorem noger;");
        l.GetNextToken();
        l.GetNextToken();
        Assert.Throws<LexingError>(l.GetNextToken);
    }

    [Fact]
    public void TestFloat() {
        Lexer l = new Lexer("2.71;");
        Assert.Equal(l.GetNextToken(), new Token(Tokens.FLOAT, "2.71"));
    }

    [Fact]
    public void TestAssign() {
        Lexer l = new Lexer("let a = \"hello world\"");
        Assert.Equal(l.GetNextToken(), new Token(Tokens.LET, "let"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.ID, "a"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.ASSIGN, "="));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.STRING, "hello world"));
    }

    [Fact]
    public void TestFunction() {
        Lexer l = new Lexer("a(n) = 5 ");
        Assert.Equal(l.GetNextToken(), new Token(Tokens.ID, "a"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.LPAREN, "("));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.ID, "n"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.RPAREN, ")"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.ASSIGN, "="));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.INTEGER, "5"));
    }

    [Fact]
    public void TestConditional() {
        Lexer l = new Lexer("if 0 then \"blob\" else \"doko\"");
        Assert.Equal(l.GetNextToken(), new Token(Tokens.IF, "if"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.INTEGER, "0"));
        l.GetNextToken();
        Assert.Equal(l.GetNextToken(), new Token(Tokens.STRING, "blob"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.ELSE, "else"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.STRING, "doko"));
    }

    [Fact]
    public void TestNewline() {
        Lexer l = new Lexer("hello\nworld");
        Assert.Equal(l.GetNextToken(), new Token(Tokens.ID, "hello"));
        Assert.Equal(l.GetNextToken(), new Token(Tokens.ID, "world"));
    }
}
