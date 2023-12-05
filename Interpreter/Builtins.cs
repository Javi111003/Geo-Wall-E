namespace Interpreter;

// builtins
//
// class [Fn]BlockNode -> evaluable body
// class [Fn] {
//     public [Fn] {
//         "[fn]",
//         args,
//         [Fn]BlockNode()
//     }
// }
class PrintBlockNode : UnaryOperation<object, object> {
    public PrintBlockNode(AST argument) : base(argument) {}

    public override object Operation(object arg) {
        Console.WriteLine(arg);
        return null;
    }
}

class Print : FunctionDeclaration {

    public Print() : base(
        "print"
    ) {}
}

class CosBlockNode : UnaryOperation<float, float> {
    public CosBlockNode(AST argument) : base(argument) {}

    public override float Operation(float arg) {
        return (float) Math.Cos(arg);
    }
}

class Cos : FunctionDeclaration {

    public Cos() : base(
        "cos"
    ) {}
}

class SinBlockNode : UnaryOperation<float, float> {
    public SinBlockNode(AST argument) : base(argument) {}

    public override float Operation(float arg) {
        return (float) Math.Sin(arg);
    }
}

class Sin : FunctionDeclaration {

    public Sin() : base(
        "sin"
    ) {}
}

class LogBlockNode : UnaryOperation<float, float> {
    public LogBlockNode(AST argument) : base(argument) {}

    public override float Operation(float arg) {
        return (float) Math.Log(arg);
    }
}

class Log : FunctionDeclaration {

    public Log() : base(
        "log"
    ) {}
}

// Adding new tokens, you say?
// nyahahahaha
class True : VariableDeclaration {

    public True() : base("True", new BoolLiteral(true)) {}
}

class False : VariableDeclaration {

    public False() : base("False", new BoolLiteral(false)) {}
}

class Undefined : VariableDeclaration {

    public Undefined() : base("undefined", new SequenceLiteral(new Terms(new List<AST>()))) {}
}

class Underscore : VariableDeclaration {

    public Underscore() : base("_", new Literal<object>(null)) {}
}
