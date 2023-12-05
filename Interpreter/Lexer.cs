using System.Text;
using System.Text.RegularExpressions;

namespace Interpreter;

public class Lexer {
    public string Text;
    protected int pos;
    public string current_char;
    public int Line;
    public int column;

    public static HashSet<string> LITERALS = new HashSet<string>{Tokens.STRING, Tokens.INTEGER, Tokens.FLOAT, Tokens.SEQUENCE_START};
    public static HashSet<string> UNARY = new HashSet<string>{Tokens.MINUS, Tokens.NOT};
    public static HashSet<string> CONDITIONALS = new HashSet<string>{Tokens.EQUALS, Tokens.HIGHER, Tokens.LOWER};
    public static HashSet<string> OPERATIONS = new HashSet<string>{
        Tokens.MULT,
        Tokens.DIV,
        Tokens.MODULO,
        Tokens.EXP,
    };
    public static Dictionary<string, Func<int, int, Token>> RESERVED_KEYWORDS = new Dictionary<string, Func<int, int, Token>>{
        {"restore", Reserved(Tokens.RESTORE)},
        {"draw", Reserved(Tokens.DRAW)},
        {"color", Reserved(Tokens.COLOR)},
        {"if", Reserved(Tokens.IF)},
        {"else", Reserved(Tokens.ELSE)},
        {"then", Reserved(Tokens.THEN)},
        {"let", Reserved(Tokens.LET)},
        {"in", Reserved(Tokens.IN)},
    };

    public static HashSet<string> HARD_CODED_BUILTINS = new HashSet<string> {
        Tokens.RESTORE,
        Tokens.DRAW,
        Tokens.COLOR,
    };

    public static Func<int, int, Token> Reserved(string name) {
        return (int Line, int column) => new Token(name, null, Line, column);
    }

    public Lexer(string Text) {
        this.Text = Text;
        this.pos = 0;
        this.current_char = this.Text[this.pos].ToString();

        this.Line = 1;
        this.column = 1;
    }

    public void Error(Exception exception) {
        Console.WriteLine(
            "Error lexing line " + this.Line.ToString() + " col " + (string) this.column.ToString()
        );
        Console.WriteLine(this.Text);
        for (int i = 0; i < this.column - 1; i++) {
            Console.Write(" ");
        }
        Console.Write("^");
        Console.WriteLine();
        throw exception;
   }

    public static bool IsDigit(string s) {
        string pattern = @"[0-9]";
        return Regex.Match(s, pattern).Success;
    }

    public static bool IsAlpha(string s) {
        // alpha or "_" actually
        string pattern = @"[A-Za-z_]";
        return Regex.Match(s, pattern).Success;
    }

    public static bool IsAlnum(string s) {
        string pattern = @"[A-Za-z0-9_]";
        return Regex.Match(s, pattern).Success;
    }

    // 

    public string GetResult(Func<string, bool> condition) {
        StringBuilder result = new StringBuilder();
        // we have en (easy) way to do pattern-matching bullshit in Csharp!
        while ((this.current_char != Tokens.EOF) && condition(this.current_char)) {
            result.Append(this.current_char);
            this.Advance();
        }

        return result.ToString();
    }

    public void Advance() {
        // it's a string
        if (this.current_char != "" && this.current_char[0] == '\n') {
            this.Line += 1;
            this.column = 0;
        }
        this.pos += 1;
        this.column += 1;
        if (this.pos > this.Text.Length - 1) {
            // EOF
            this.current_char = "";
        }
        else {
            this.current_char = this.Text[this.pos].ToString();
        }
    }

    public string Peek() {
        int peek_pos = this.pos + 1;
        if (peek_pos > this.Text.Length - 1) {
            // EOF
            // XXX could this cause a bug with composite characters at the end
            // of the input? no sane persion would do that but...
            return "";
        }
        else {
            return this.Text[peek_pos].ToString();
        }
    }

    public Token Number() {
        string integer = this.GetResult((string s) => IsDigit(s));

        // could be float
        if (this.current_char == "." && IsDigit(this.Peek())) {
            this.Advance();
            string mantisa = this.GetResult((string s) => IsDigit(s));
            return new Token(Tokens.FLOAT, integer.ToString() + "." + mantisa.ToString(), this.Line, this.column);
        }
        return new Token(Tokens.INTEGER, integer, this.Line, this.column);

    }

    public Token String() {
        // pass '"'
        this.Advance();
        string result = this.GetResult((string s) => s != "\"");
        
        if (this.current_char == Tokens.EOF) {
            this.Error(new LexingError("Unterminated string literal"));
        }

        // pass final '"'
        this.Advance();

        return new Token(Tokens.STRING, result, this.Line, this.column);
    }

    public Token Id() {
        // Namespaces and reserved keyworkds
        
        Token token;

        string result = this.GetResult((string s) => IsAlnum(s));
        if (RESERVED_KEYWORDS.ContainsKey(result)) {
            token = RESERVED_KEYWORDS[result](this.Line, this.column);
        }
        else {
            token = new Token(Tokens.ID, result, this.Line, this.column);
        }

        return token;
    }

    public Token GetNextToken() {
        while (this.current_char != Tokens.EOF) {
            if (this.current_char == " ") {
                // skip whitespace
                this.GetResult((string s) => s == " ");
                continue;
            }
            else if (this.current_char[0] == '\n') {
                this.Advance();
                continue;
            }

            // no namespace or reserved keyword can start with a number
            if (IsAlpha(this.current_char)) {
                return this.Id();
            }

            if (IsDigit(this.current_char)) {
                Token num = this.Number();
                if (IsAlpha(this.current_char)) {
                    this.Error(new LexingError("Invalid numeric literal"));
                }

                return num;
            }

            if (this.current_char == "\"") {
                return this.String();
            }

            // single token
            // in short, it's just a symbol and the value of the token it's the symbol itself
            // don't forget there are composite tokens as well as single-character tokens
            string token_repr = Tokens.FromValue(this.current_char, this.Peek());
            if (token_repr == null) {
                this.Error(new LexingError($"Invalid character {(int) this.current_char[0]}"));
            }

            if (token_repr == "//") {
                // Es un comentario por tanto se ignora el resto de la linea
                this.GetResult((string s) => s[0] != '\n');
                // we are in newline
                // advance
                this.Advance();

                return this.GetNextToken();
            }

            // special handling for composite tokens
            for (int i = 0; i < token_repr.Length; i++) {
                this.Advance();
            }
          
            return new Token(token_repr, null, this.Line, this.column);
        }
        return new Token(Tokens.EOF, null, this.Line, this.column);
    }
    public Token[] GetAllTokens()
    {
        List<Token>tokens = new List<Token>();
        while (this.current_char != "") {
            tokens.Add(GetNextToken());
        }
        // add EOF
        tokens.Add(new Token(Tokens.EOF, null, this.Line, this.column));
        return tokens.ToArray();
    }
}
