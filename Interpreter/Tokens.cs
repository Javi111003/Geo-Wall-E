using System;
using System.Reflection;

namespace Interpreter;

public static class Tokens {
    public static string ID = "ID";
    public static string INTEGER = "INTEGER";
    public static string FLOAT = "FLOAT";
    public static string STRING = "STRING";
    public static string PLUS = "+";
    public static string MINUS = "-";
    public static string MULT = "*";
    public static string DIV = "/";
    public static string COMMENT = "//";
    public static string MODULO = "%";
    public static string EXP = "^";
    public static string LPAREN = "(";
    public static string RPAREN = ")";
    public static string HIGHER = ">";
    public static string EQUALS = "==";
    public static string HIGHEREQUAL = ">=";
    public static string LOWEREQUAL = "<=";
    public static string LOWER = "<";
    public static string ASSIGN = "=";
    public static string DOT = ".";
    public static string END = ";";
    public static string EOF = "";
    public static string DRAW = "draw";
    public static string COLOR = "color";
    public static string RESTORE = "restore";
    public static string UNDEFINED = "undefined";
    public static string THEN = "then";
    public static string ELSE = "else";
    public static string IF = "if";
    public static string LET = "let";
    public static string IN = "in";
    public static string QUOTATION = "\"";
    public static string COMMA = ",";
    public static string NOT = "!";
    public static string SEQUENCE_END = "}";
    public static string SEQUENCE_START = "{";


    public static string FromValue(string token1, string token2) {
        FieldInfo[] fields = typeof(Tokens).GetFields();

        foreach(FieldInfo field in fields) {
            string composite = token1 + token2;
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
    // this can't be "Type" because it could be confused with AST.Type
    // (so when I Ctrl+F "type" i know it will be token.type and not AST.Type)
    // val is also not capitalized for consistency
    public string type;
    public string val;
    public int Line;
    public int column;

    public Token(string type ,string val = null, int Line=0, int column=0) {
        this.type = type;
        this.Line = Line;
        this.column = column;
        if (val is null) {
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
        return this.type == other.type && this.val == other.val;
    }
}

