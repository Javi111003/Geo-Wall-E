using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Interpreter;

public class LexingError: Exception {
    public LexingError() {}
    public LexingError(string message): base(message) {}
    public LexingError(string message, Exception inner): base(message, inner) {}
}

public static class Tokens {
    public static string ID = "ID";
    public static string INTEGER = "INTEGER";
    public static string FLOAT = "FLOAT";
    public static string STRING = "STRING";
    public static string PLUS = "+";
    public static string  MINUS = "-";
    public static string MULT = "*";
    public static string DIV = "/";
    public static string MODULO = "%";
    public static string EXP = "^";
    public static string LPAREN = "(";
    public static string RPAREN = ")";
    public static string HIGHER = ">";
    public static string EQUALS = "==";
    public static string LOWER = "<";
    public static string ASSIGN = "=";
    public static string FINLINE = "=>";
    public static string DOT = ".";
    public static string END = ";";
    public static string EOF = "";
    public static string VAR = "var";
    public static string LET = "let";
    public static string IN = "in";
    public static string IF = "if";
    public static string  ELSE = "else";
    public static string  FUNCTION = "function";
    public static string QUOTATION = "\"";
    public static string  COMMA = ",";
    public static string  NOT = "!";

    public static string FromValue(string token1, string token2) {
        FieldInfo[] fields = typeof(Tokens).GetFields();

        string composite = token1 + token2;
        foreach(FieldInfo field in fields) {
            if ((string) field.GetValue(null) == composite) {
                return composite;
            }
        }

        foreach(FieldInfo field in fields) {
            if ((string) field.GetValue(null) == token1) {
                return token1;
            }
        }

        return null;
    }
}

public class Token {
    public string type;
    public string val;

    public Token(string type_, string val = "") {
        this.type = type_;
        if (val == "") {
            this.val = this.type;
        }
        else {
            this.val = val;
        }
    }

    public override string ToString() {
        return "<Token(" + this.type + ", " + this.val + ")>";
    }

    public override bool Equals(object obj) => this.Equals(obj as Token);
    public bool Equals(Token other) {
        return this == other;
    }

    public static bool operator ==(Token left, Token right) {
        return left.type == right.type && left.val == right.val;
    }

    // why do I have to define it???
    public static bool operator !=(Token left, Token right) => !(left == right);
}

public class Lexer {
    public string text;
    public int pos;
    public string current_char;
    public int line;
    public int column;

    public static HashSet<string> LITERALS = new HashSet<string>{Tokens.STRING, Tokens.INTEGER, Tokens.FLOAT};
    public static HashSet<string> UNARY = new HashSet<string>{Tokens.MINUS, Tokens.NOT};
    public static HashSet<string> CONDITIONALS = new HashSet<string>{Tokens.EQUALS, Tokens.HIGHER, Tokens.LOWER};
    public static HashSet<string> OPERATIONS = new HashSet<string>{
        Tokens.MULT,
        Tokens.DIV,
        Tokens.MODULO,
        Tokens.EXP,
    };
    public static Dictionary<string, Token> RESERVED_KEYWORDS = new Dictionary<string, Token>{
        {"in", new Token(Tokens.IN)},
        {"let", new Token(Tokens.LET)},
        {"var", new Token(Tokens.VAR)},
        {"function", new Token(Tokens.FUNCTION)},
        {"if", new Token(Tokens.IF)},
        {"else", new Token(Tokens.ELSE)}
    };

    public Lexer(string text) {
        this.text = text;
        this.pos = 0;
        this.current_char = this.text[this.pos].ToString();

        this.line = 1;
        this.column = 1;
    }

    public void Error(Exception exception) {
        Console.WriteLine(
            "Error lexing line " + this.line.ToString() + " col " + (string) this.column.ToString()
        );
        Console.WriteLine(this.text);
        for (int i = 0; i < this.column - 1; i++) {
            Console.Write(" ");
        }
        Console.Write("^");
        Console.WriteLine();
        throw exception;
   }

    public static bool PatternMatching(string condition, string character) {
        if (condition == "space") {
            return character == " ";
        }
        else if (condition == "not_quotation") {
            return character != "\"";
        }
        else if (condition == "digit") {
            return IsDigit(character);
        }
        else if (condition == "alnum") {
            return IsAlnum(character);
        }
        else {
            throw new ArgumentException("Invalid condition" + condition);
        }
    }

    public static bool IsDigit(string s) {
        string pattern = @"[0-9]";
        return Regex.Match(s, pattern).Success;
    }

    public static bool IsAlpha(string s) {
        string pattern = @"[A-Za-z]";
        return Regex.Match(s, pattern).Success;
    }

    public static bool IsAlnum(string s) {
        string pattern = @"[A-Za-z0-9]";
        return Regex.Match(s, pattern).Success;
    }

    // 

    public string GetResult(string condition) {
        StringBuilder result = new StringBuilder();
        bool pattern_matches = PatternMatching(condition, this.current_char);
        // XXX I wish we had (easy) pattern-matching bullshit in Csharp...
        while ((this.current_char != Tokens.EOF) && pattern_matches) {
            result.Append(this.current_char);
            this.Advance();
            pattern_matches = PatternMatching(condition, this.current_char);
        }

        return result.ToString();
    }

    public void Advance() {
        if (this.current_char == "\n") {
            this.line += 1;
            this.column = 0;
        }

        this.pos += 1;
        this.column += 1;
        if (this.pos > this.text.Length - 1) {
            // EOF
            this.current_char = "";
        }
        else {
            this.current_char = this.text[this.pos].ToString();
        }
    }

    public string Peek() {
        int peek_pos = this.pos + 1;
        if (peek_pos > this.text.Length - 1) {
            // EOF
            // XXX could this cause a bug with composite characters at the end
            // of the input? no sane persion would do that but...
            return "";
        }
        else {
            return this.text[peek_pos].ToString();
        }
    }

    public Token Number() {
        string integer = this.GetResult("digit");

        // could be float
        if (this.current_char == "." && IsDigit(this.Peek())) {
            this.Advance();
            string mantisa = this.GetResult("digit");
            return new Token(Tokens.FLOAT, integer.ToString() + "." + mantisa.ToString());
        }
        return new Token(Tokens.INTEGER, integer);

    }

    public Token String() {
        // pass '"'
        this.Advance();
        string result = this.GetResult("not_quotation");
        
        if (this.current_char == Tokens.EOF) {
            this.Error(new LexingError("Unterminated string literal"));
        }

        // pass final '"'
        this.Advance();

        return new Token(Tokens.STRING, result);
    }

    public Token Id() {
        // Namespaces and reserved keyworkds
        
        Token token;

        string result = this.GetResult("alnum");
        if (RESERVED_KEYWORDS.ContainsKey(result)) {
            token = RESERVED_KEYWORDS[result];
        }
        else {
            token = new Token(Tokens.ID, result);
        }

        return token;
    }

    public Token GetNextToken() {
        while (this.current_char != Tokens.EOF) {
            if (this.current_char == " ") {
                // skip whitespace
                this.GetResult("space");
                continue;
            }

            // no namespace or reserved keyword can start with a number
            if (IsAlpha(this.current_char)) {
                return this.Id();
            }

            if (IsDigit(this.current_char)) {
                return this.Number();
            }

            if (this.current_char == "\"") {
                return this.String();
            }

            // single token
            // in short, it's just a symbol and the value of the token it's the symbol itself
            // don't forget there are composite tokens as well as single-character tokens
            string token_repr = Tokens.FromValue(this.current_char, this.Peek());
            if (token_repr == null) {
                this.Error(new LexingError("Invalid character"));
            }

            // special handling for composite tokens
            for (int i = 0; i < token_repr.Length; i++) {
                this.Advance();
            }

            return new Token(token_repr);
        }
        return new Token(Tokens.EOF);
    }
}
