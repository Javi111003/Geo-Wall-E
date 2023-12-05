using System;

namespace Interpreter;


// lexer

public class LexingError: Exception {
    public LexingError() {}
    public LexingError(string message): base(message) {}
    public LexingError(string message, Exception inner): base(message, inner) {}
}

// parse

public class NameError : Exception {
    public NameError() {}
    public NameError(string message): base(message) {}
    public NameError(string message, Exception inner): base(message, inner) {}
}

public class UnexpectedToken : Exception {
    public UnexpectedToken() {}
    public UnexpectedToken(string message): base(message) {}
    public UnexpectedToken(string message, Exception inner): base(message, inner) {}
}

public class SyntaxError : Exception {
    public SyntaxError() {}
    public SyntaxError(string message): base(message) {}
    public SyntaxError(string message, Exception inner): base(message, inner) {}
}

public class TypeError : Exception {
    public TypeError() {}
    public TypeError(string message): base(message) {}
    public TypeError(string message, Exception inner): base(message, inner) {}
}

public class RuntimeError : Exception {
    public RuntimeError() {}
    public RuntimeError(string message): base(message) {}
    public RuntimeError(string message, Exception inner): base(message, inner) {}
}
