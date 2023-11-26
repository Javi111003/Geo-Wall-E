using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Interpreter;

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

public static class BestGuess {
    public static int Expression = 1;
    public static int Declaration = 2;
}

public class Context : Dictionary<string, dynamic> {

    public dynamic this[string key] {
        get {
            try {
                return base[key];
            }
            catch(System.Collections.Generic.KeyNotFoundException) {
                throw new NameError(key + " is not defined");
            }
        }
        set {
            base[key] = value;
        }
    }

    public Context Clone() {
        Context ret = new Context();
        foreach(KeyValuePair<string, dynamic> entry in this) {
            ret.Add(entry.Key, (dynamic) entry.Value);
        }
        return ret;
    }
}

public class AST {

    protected string type;

    public virtual string Type {
        get {return this.type;}
        set {this.type = value;}
    }

    public AST(string Type="DYNAMIC") {
        this.Type = Type;
    }

    public virtual dynamic Eval(Context ctx) {
        return "";
    }

    public override string ToString() {
       return this.Eval(new Context()).ToString();
    }

    // type check
    public virtual Exception Check() {
        return null;
    }
}

public class AST<T>: AST {

    public static string STRING = Tokens.STRING;
    public static string INTEGER = Tokens.INTEGER;
    public static string FLOAT = Tokens.FLOAT;
    public static string BOOL = "BOOL";
    public static string DYNAMIC = "DYNAMIC";
    public static string SEQUENCE = "SEQUENCE";

    public static HashSet<string> FloatOp = new HashSet<string>{FLOAT, INTEGER};
    public static HashSet<string> BoolOp = new HashSet<string>{FLOAT, INTEGER, BOOL, STRING};
    public static HashSet<string> StrOp = new HashSet<string>{STRING};
    public static HashSet<string> SeqOp = new HashSet<string>{SEQUENCE};

    public static Dictionary<string, HashSet<string>> Compatible = new Dictionary<string, HashSet<string>>{
        {FLOAT, FloatOp},
        {BOOL, FloatOp},
        {STRING, StrOp},
    };

    public AST(string Type): base(Type) {}

    public static Dictionary<string, Type> Types = new Dictionary<string, Type>{
        {STRING, typeof(string)},
        {INTEGER, typeof(int)},
        {FLOAT, typeof(float)},
        {BOOL, typeof(bool)},
        // can't typeof(dynamic)
        // this also means we don't care about the type (print)
        {DYNAMIC, typeof(object)}
        // not for SEQUENCE
    };
    public static Dictionary<Type, string> RevTypes = new Dictionary<Type, string>{
        {typeof(string), STRING},
        {typeof(int), INTEGER},
        {typeof(float), FLOAT},
        {typeof(bool), BOOL},
        {typeof(object), DYNAMIC}
        // no reverse search for typeof(object)
    };

    public static string ToStr() {
        return RevTypes[typeof(T)];
    }
    public static string ToStr(Type type) {
        return RevTypes[type];
    }
    public static Type ToType(string str) {
        return Types[str];
    }

    public static Type InferType(AST<object> ast) {
        return Types[ast.Type];
    }
}

public abstract class Literal<T>: AST<T> {
    protected T val;

    public Literal(T val, string Type): base(Type){
        this.val = val;
    }

    public T Val() {
        return this.val;
    }

    public override dynamic Eval(Context ctx) {
        return this.val;
    }

    // no need to override check
}

public class StringLiteral: Literal<string> {

    public StringLiteral(string val): base(val, AST<object>.STRING) {}
}

public class IntLiteral : Literal<int> {

    public IntLiteral(int val): base(val, AST<object>.INTEGER) {}
}

public class FloatLiteral : Literal<float> {

    public FloatLiteral(float val): base(val, AST<object>.FLOAT) {}
}

public class BoolLiteral : Literal<bool> {

    public BoolLiteral(bool val): base(val, AST<object>.BOOL) {}
}

// an IEnumerator but not an IEnumerator
public class Terms: AST {
    int start;
    int end;
    int index;
    protected List<AST> ls;
    // we don't know looking at the fields
    // this is used to sum sequences
    public bool IsInfinite;
    Func<int, dynamic> term;

    // lambda copy for the class
    // we have to ways to define the class
    // (we could separate the classes in Terms and Terms<int>) but it's "three strikes and you are out"
    // not two
    Func<Terms> copy;

    public override string Type {
        get {
            if (this.start == this.end) {
                return AST<object>.INTEGER;
            }
            return this.term(0).Type;
        }
    }

    public Terms(List<AST> ls) {
        this.index = -1;
        this.start = 0;
        this.end = ls.Count();
        this.ls = ls;
        this.term = (int i) => {return ls[i];};
        this.IsInfinite = false;

        // know which one to call
        this.copy = () => new Terms(ls);
    }

    public Terms(int start, int? end): base(AST<int>.ToStr()) {
        this.index = start - 1;
        this.start = start;
        if (end is null) {
            this.end = Int32.MaxValue;
            this.IsInfinite = true;
        }
        else {
            this.end = (int) end;
            this.IsInfinite = false;
        }
        this.term = (int index) => {return new IntLiteral(index);};

        this.copy = () => new Terms(start, end);

        this.ls = null;
    }

    public Terms Clone() {
        return this.copy();
    }

    public override dynamic Eval(Context ctx) {
        if (this.MoveNext()) {
            return this.Current.Eval(ctx);
        }
        return null;
    }

    public bool MoveNext() {
        index += 1;
        // do not include range end [start, end)
        if (this.index >= end) {
            return false;
        }

        return true;
    }

    public AST Current {
        get {
            return this.term(this.index);
        }
    }

    public IEnumerator<AST> GetEnumerator() {
        for (int i = this.index + 1; i < this.end; i++) {
            yield return this.term(i);
        }
    }

    public override string ToString() {
        StringBuilder str = new StringBuilder("{");
        if (!this.IsInfinite) {
            foreach(AST item in this) {
                str.Append(item.ToString() + ", ");
            }
        }
        else {
            str.Append($"{this.start}...");
        }
        str.Append("}");
        return str.ToString();
    }

}

// block node... reimagined
public class SequenceLiteral : Literal<Terms> {

    public SequenceLiteral(Terms val): base(val, AST<object>.SEQUENCE) {}

    public SequenceLiteral Clone() {
        return new SequenceLiteral(this.val.Clone());
    }

    public override dynamic Eval(Context ctx) {
        return this;
    }

    public override string ToString() {
        return this.val.ToString();
    }
}

// <In, Out>
public abstract class BinaryOperation<T, R>: AST<R> {
    public AST left;
    public AST right;

    public BinaryOperation(AST left, AST right): base(AST<R>.ToStr(typeof(R))) {
        this.left = left;
        this.right = right;
    }

    public override dynamic Eval(Context ctx) {
        // ValidateArgs -> See Type and check if they are valid before eval
        try {
            return this.Operation(this.left.Eval(ctx), this.right.Eval(ctx));
        }
        catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) {
            string msg = $"Unsupported operand type(s) for {this.GetType().Name.ToLower()}: {this.left.Type} and {this.right.Type}";
            throw new RuntimeError(msg);
        }
    }

    public override Exception Check() {
        bool dynamic_expr = this.right.Type == AST<object>.DYNAMIC || this.left.Type == AST<object>.DYNAMIC;
        bool right_type = AST<object>.Compatible[this.Type].Contains(this.left.Type) && AST<object>.Compatible[this.Type].Contains(this.right.Type);

        if (dynamic_expr || right_type) {
            return null;
        }

        string msg = $"Unsupported operand type(s) for {this.GetType().Name.ToLower()}: {left.GetType()} and {right.GetType()} (expected {AST<R>.ToStr(typeof(R))})";
        return new TypeError(msg);
    }

    public abstract R Operation(T a, T b);
}

public class Sum : BinaryOperation<float, float> {

    public Sum(AST left, AST right): base(left, right) {}
   
    public override float Operation(float a, float b) {
        return a + b;
    }
}

public class Substraction : BinaryOperation<float, float> {

    public Substraction(AST left, AST right): base(left, right) {}
   
    public override float Operation(float a, float b) {
        return a + -b;
    }
}
public class Division : BinaryOperation<float, float> {

    public Division(AST left, AST right): base(left, right) {}
   
    public override float Operation(float a, float b) {
        return a / b;
    }
}
public class Mult : BinaryOperation<float, float> {
   
    public Mult(AST left, AST right): base(left, right) {}

    public override float Operation(float a, float b) {
        return a * b;
    }
}
public class Modulo : BinaryOperation<float, float> {
   
    public Modulo(AST left, AST right): base(left, right) {}

    public override float Operation(float a, float b) {
        return a % b;
    }
}
public class Exp : BinaryOperation<float, float> {

    public Exp(AST left, AST right): base(left, right) {}
   
    public override float Operation(float a, float b) {
        return (float) System.Math.Pow(a, b);
    }
}
public class Equals : BinaryOperation<float, bool> {

    public Equals(AST left, AST right): base(left, right) {}
   
    public override bool Operation(float a, float b) {
        return a == b;
    }
}
public class Higher : BinaryOperation<float, bool> {

    public Higher(AST left, AST right): base(left, right) {}
   
    public override bool Operation(float a, float b) {
        return a > b;
    }
}
public class HigherEqual : BinaryOperation<float, bool>
{

    public HigherEqual(AST left, AST right) : base(left, right) { }

    public override bool Operation(float a, float b)
    {
        return a >= b;
    }
}
public class Lower : BinaryOperation<float, bool> {

    public Lower(AST left, AST right): base(left, right) {}
   
    public override bool Operation(float a, float b) {
        return a < b;
    }
}
public class LowerEqual : BinaryOperation<float, bool>
{

    public LowerEqual(AST left, AST right) : base(left, right) { }

    public override bool Operation(float a, float b)
    {
        return a <= b;
    }
}

//
public abstract class UnaryOperation<T, R> : AST<R> {

    public AST block;

    public override string Type {
        get {
            return this.block.Type;
        }
    }

    public UnaryOperation(AST block): base(AST<R>.ToStr()) {
        this.block = block;
    }
    
    public override dynamic Eval(Context ctx) {
        return this.Operation(this.block.Eval(ctx));
    }

    public abstract R Operation(T a);

    public void Check() {
        bool dynamic_expr = this.block.Type == AST<object>.DYNAMIC;
        bool right_type = this.block.Type == AST<R>.ToStr(typeof(R));

        if ((dynamic_expr || right_type)) {
            return;
        }

        string msg = $"Unsupported operand type(s) for {this.GetType().Name.ToLower()}: {this.block.Type}";
        throw new RuntimeError(msg);
    }
}

public class ChangeSign : UnaryOperation<float, float> {

    public ChangeSign(AST block): base(block) {}
    
    public override float Operation(float a) {
        return -a;
    }
}

public class Negate : UnaryOperation<float, bool> {

    public Negate(AST block): base(block) {}

    // c# gets confused with the dynamic type for some reason
    public override dynamic Eval(Context ctx) {
        return this.Operation(this.block.Eval(ctx));
    }

    public override bool Operation(float a) {
        return this.Operation(Convert.ToBoolean(a));
    }

    public bool Operation(bool a) {
        return !a;
    }
}

// these AST's types depend of something so Type is a property
public class VariableDeclaration: AST {

    public string name;
    public AST expression;

    public override string Type {
        get {
            return this.expression.Type;
        }
    }

    public VariableDeclaration(string name, AST expression) {
        this.name = name;
        this.expression = expression;
    }

    public override dynamic Eval(Context ctx) {
        ctx[this.name] = this.expression.Eval(ctx);
        return null;
    }

    public override string ToString() {
        if (this.expression is null) {
            return "<(Variable) [name: " + this.name + ", value: undefined]>";
        }
        return "<(Variable) [name: " + this.name + ", value: " + this.expression.ToString() + "]>";
    }
}

// this depends of the type of the declaration
public class Variable : AST {

    public string name;
    // XXX notice this is not VariableDeclaration<dynamic>
    // we only care about VariableDeclaration.expression so this is what is here
    public VariableDeclaration declaration;

    public override string Type{
        get {
            if (declaration is null) {
                return AST<object>.DYNAMIC;
            }
            return declaration.Type;
        }
    }

    public Variable(string name) {
        this.name = name;
        // we don't know beforehand
        this.declaration = null;
    }

    public override dynamic Eval(Context ctx) {
        // should be a literal
        return ctx[this.name];
    }

    public override string ToString() {
        return "<(Variable) [name: " + this.name + "]>";
    }
}

// always dynamic; we just don't know
// that said we can change it (in case of sequences it's a must)
public class BlockNode: AST {
    // Contains a set of evaluables
    // only the last block is returned to the interpreter

    public List<AST> blocks;

    public BlockNode(List<AST> blocks) {
        this.blocks = blocks;
    }

    public override dynamic Eval(Context ctx) {
        int counter = blocks.Count();
        foreach(AST block in this.blocks) {
            if (counter == 1) {
                var bl = block.Eval(ctx);
                return bl;
            }
            block.Eval(ctx);
            counter -= 1;
        }
        return null;
    }

    public override Exception Check() {
        foreach(AST item in this.blocks) {
            Exception? exc = item.Check();
            if (!(exc is null)) {
                return exc;
            }
        }
        return null;
    }

    public override string ToString() {
        return this.blocks.ToString();
    }
}

public class FunctionDeclaration: AST {

    public string name;
    // we don't konw the types of each argument
    public BlockNode args;
    public AST body;

    public override string Type{
        get {
            return this.body.Type;
        }
    }

    public FunctionDeclaration(string name, BlockNode args, AST body) {
        this.name = name;
        this.args = args;
        this.body = body;
    }

    public override dynamic Eval(Context ctx) {
        ctx[this.name] = this;
        return null;
    }

    public override string ToString() {
        return $"<(FunctionDeclaration) [name: {this.name}, args: {this.args}, body: {this.body} type: {this.Type}]>";
     }


    public override Exception Check() {
        return this.body.Check();
    }
}

// same as with Variable
public class Function : AST {
    public string name;
    public BlockNode args;

    public FunctionDeclaration declaration;

    public override string Type {
        get {
            if (this.declaration is null) {
                return AST<object>.DYNAMIC;
            }
            return this.declaration.Type;
        }
    }

    public Function(string name, BlockNode args) {
        this.name = name;
        this.args = args;

        this.declaration = null;
    }

    public override dynamic Eval(Context ctx) {
        FunctionDeclaration fun_decl = (FunctionDeclaration) ctx[this.name];
        BlockNode fun_args = fun_decl.args;

        // know if we have to infer the type (or if we already inferred it from the declaration)
        bool is_dynamic = this.Type == AST<object>.DYNAMIC;
        
        Context fun_ctx = ctx.Clone();
        for (int i = 0; i < fun_args.blocks.Count(); i++) {
            Variable variable = (Variable) fun_args.blocks[i];
            try {
                AST expression = this.args.blocks[i];
                fun_ctx[variable.name] = expression.Eval(ctx);
            }
            catch (Exception) {
                throw new RuntimeError($"Too many/few arguments for {this.name}");
            }
        }

        // allow recursivity (explicitly)
        //fun_ctx[this.name] = fun_decl;

        return fun_decl.body.Eval(fun_ctx);
   }


    public override string ToString() {
        return $"<(Function) [name: {this.name}, args: {this.args}, type: {this.Type}]>";
    }

    // TODO we can pass parameters to check or do something to check the declaration
    // based on the variables passed
    // public override Exception Check() {
}

public class Lambda : AST {
    // let-in expression

    public BlockNode variables;
    public AST body;

    public override string Type {
        get {
            return body.Type;
        }
    }

    public Lambda(BlockNode variables, AST body) {
        this.variables = variables;
        this.body = body;
    }

    public override dynamic Eval(Context ctx) {
        Context local_ctx = ctx.Clone();

        // https://github.com/matcom/programming/tree/main/projects/hulk#variables
        // "( ... ) Fuera de una expresión let-in las variables dejan de existir. ( ... )"
        // thus, we declare variables inside the scope of the lambda
        this.variables.Eval(local_ctx);

        var res = this.body.Eval(local_ctx);

        return res;
    }


    public override Exception Check() {
        return this.body.Check();
    }
}

public class Conditional : AST {
    // we convert it to bool
    AST hypothesis;
    AST thesis;
    AST antithesis;

    public override string Type {
        get {
            return this.thesis.Type;
        }
    }

    public Conditional(
        AST hypothesis,
        AST thesis,
        AST antithesis
    ) {
        this.hypothesis = hypothesis;
        this.thesis = thesis;
        this.antithesis = antithesis;
   }

    public override dynamic Eval(Context ctx) {
        bool res = Convert.ToBoolean(this.hypothesis.Eval(ctx));
        if (res) {
            return this.thesis.Eval(ctx);
        }
        return this.antithesis.Eval(ctx);
    }

    public override Exception Check() {
        // TODO thesis and antithesis have the same type (or dynamic etc etc)
        // NOTE we need not to check the type of the hypothesis--everything evals to either True or False
        // almost the same as binaryops.Check()

        bool dynamic_expr = this.thesis.Type == AST<object>.DYNAMIC || this.antithesis.Type == AST<object>.DYNAMIC;
        bool right_type = AST<object>.Compatible[this.thesis.Type].Contains(this.antithesis.Type);

        if (dynamic_expr || right_type) {
            return null;
        }

        string msg = $"Type mismatch inside Conditional statement : {this.thesis.Type} != {this.antithesis.Type}";
        return new TypeError(msg);
    }
}

public class Parser {
    // FunctionMap -> Dict with declared functions (they are global)
    // VariableMap -> Dict with global variables <string, List<VariableDeclaration>>
    //
    // We can use them to infer the type
    Lexer lexer;
    Token current_token;

    Token[] tokens;
    int index;

    // global
    public Context global_context;
    public Context local_context;

    public Parser(Lexer lexer, Context context = null) {
        this.index = 0;
        this.lexer = lexer;
        this.tokens = this.lexer.GetAllTokens();

        this.current_token = this.GetNextToken();

        if (context is null) {
            this.global_context = this.local_context = new Context();
        }
        else {
            this.global_context = this.local_context = context;
        }
    }

    public Token GetNextToken() {
        return this.tokens[this.index++];
    }

    public void MoveBack(int steps = 1) {
        //regresar al token anterior o a los anteriores si no matchea lo que esperabamos 

        this.index -= steps;
        this.current_token = this.tokens[index];
   }

   public Token Peek(int steps = 1) {
       if (this.index < 0 || (this.index > this.tokens.Length)) {
           // return EOF so it doesn't match anything (except, maybe ending whatever loop is using peek)
           return new Token(Tokens.EOF);
       }
       return this.tokens[this.index + steps];
   }

    public void Error(Exception exception) {
        Console.WriteLine($"Error parsing line {this.current_token.Line} col {this.current_token.column}");
        // XXX write last line
        Console.WriteLine(this.lexer.Text);
        for (int i = 0; i < this.current_token.column - 2; i++) {
            Console.Write(" "); 
        }
        Console.Write("^");
        Console.WriteLine(); 
        throw exception;
    }

    public void Eat(string token_type) {
        if (this.current_token.type == token_type) {
            this.current_token = this.GetNextToken();
        }
        else {
            this.Error(new SyntaxError($"Expected {token_type} found {this.current_token.type}."));
        }
    }

    public AST TypeFor(string name) {
        if (this.local_context.ContainsKey(name)) {
            return this.local_context[name];
        }
        // XXX builtins
        return null;
    }

    public void Add(string ast) {
        return;
    }

    public BlockNode Arguments() {
        if (this.current_token.type != Tokens.LPAREN) {
            return null;
        }

        this.Eat(Tokens.LPAREN);
        List<AST> args = new List<AST>();
        if (this.current_token.type == Tokens.RPAREN) {
            this.Eat(Tokens.RPAREN);
            return new BlockNode(new List<AST>());
        }

       args.Add(this.Expr());
       while (this.current_token.type == Tokens.COMMA) {
           this.Eat(Tokens.COMMA);
           args.Add(this.Expr());
       }
       this.Eat(Tokens.RPAREN);

       return new BlockNode(args);
    }

    public AST Namespace(string? name=null, bool local=false) {
        // XXX could be functiof of one argument
        // draw "loli"
        if (name is null) {
            name = this.current_token.val;

            this.Eat(Tokens.ID);
        }

        AST node;
        BlockNode args = this.Arguments();
        if (args is not null) {
            node = new Function(name, args);
        }
        else {
            VariableDeclaration declaration = (VariableDeclaration) this.TypeFor(name);
            Variable var_node = new Variable(name);
            node = var_node;
            var_node.declaration = declaration;
        }
        return node;
    }

    public Lambda Letin() {
        // meme lambda-like aberration with bizarre use-cases (forma enervante)
        this.Eat(Tokens.LET);
        var nodes = new List<AST>();
        // we want a separate context for local vars in the lambda
        this.local_context = this.global_context.Clone();
        while (this.current_token.type != Tokens.IN) {
            nodes.Add(this.ParseNode());
            this.Eat(Tokens.END);
        }
        this.Eat(Tokens.IN);

        var node = new Lambda(new BlockNode(nodes), this.Expr());

        // restore old context
        this.local_context = this.global_context;

        return node;
    }

    protected (string, string?) DeclareFirst(List<AST> variables, List<string> names) {
        // helper function to declare a variable
        // the first time it can have a type before the name 
        // so it's an special case
        //
        // we modify "variables" and "names"
        //
        // FIXME fix this shite

        string name = this.current_token.val;

        this.Eat(Tokens.ID);
        // check sequence
        SequenceLiteral seq = null;
        string type = null;
        if (this.current_token.type == Tokens.SEQUENCE_START) {
            seq = (SequenceLiteral) this.LiteralNode();
            // the first string is the type
            type = name;
            name = this.current_token.val;
            this.Eat(Tokens.ID);
        }
        else if (this.current_token.type == Tokens.ID) {
            // NOTE we duplicate this code because if a sequence was declared here 
            // it has to be followed by an ID or else is a syntax error
            
            // name
            // what we got before was the type
            type = name;
            name = this.current_token.val;
            this.Eat(Tokens.ID);
        }
        else if (this.current_token.type == Tokens.LPAREN) {
            // fug, it's a function
            return (name, null);
        }

        AST val = null;
        if (this.current_token.type == Tokens.ASSIGN) {
            // asignation
            this.Eat(Tokens.ASSIGN);
            val = this.Expr();
        }
        else if (type is not null) {
            // declaration
            // [type] {args} [name]; is translated to name = type(args)
            // where type is a function
            // thus
            Function fun = new Function(type, new BlockNode(new List<AST>{seq}));
            val = fun;

        }
        VariableDeclaration variable = new VariableDeclaration(name, val);
        variables.Add(variable);

        // add to context
        this.local_context[variable.name] = variable;

        names.Add(name);

        return (name, type);
    }

    public BlockNode Declaration() {
        List<string> names = new List<string>();
        List<AST> variables = new List<AST>();

        (string name, string type) = this.DeclareFirst(variables, names);
        if (this.current_token.type == Tokens.LPAREN) {
            // function
            return new BlockNode(new List<AST>{this.FunctionDecl(name)});
        }
        VariableDeclaration variable;


        AST val = null;
        while (this.current_token.type == Tokens.COMMA) {
            this.Eat(Tokens.COMMA);
            name = this.current_token.val;
            names.Add(name);

            this.Eat(Tokens.ID);

            if (this.current_token.type == Tokens.ASSIGN) {
                this.Eat(Tokens.ASSIGN);
                val = this.Expr();
            }
            else {
                if (type is null) {
                    // undefined
                    val = null;
                }
                else {
                    Function fun = new Function(type, new BlockNode(new List<AST>()));
                    val = fun;
                }
            }


            variable = new VariableDeclaration(name, val);
            variables.Add(variable);


            // add to context
            this.local_context[variable.name] = variable;
        }

        // sequence declaration
        // this doesn't modify the original seq
        // NOTE we already evaluated the first element so if there is not a second
        // one we can't do this; hence the null check
        // don't worry about the first one not being taken into account--the list with
        // all variables is passed to the function handling the special case (DeclareFirst)
        if (!(val is null) && (val.Type == AST<object>.SEQUENCE)) {
            SequenceLiteral seq = (SequenceLiteral) val;
            var sequence = seq.Clone();

            // we pass the reference to all the variables so when they are evaluated
            // we modify the index @ Sequence.val and give a different value each time
            Terms term = sequence.Val();
            int counter = 0;
            foreach(VariableDeclaration item in variables) {
                if (counter == (variables.Count() - 1)) {
                    // minus the last one
                    // pass a reference to the cloned sequence to the last var
                    item.expression = sequence;
                    break;
                }
                // reference to the term that will return a literal when evaluated
                item.expression = term;
                counter += 1;
            }
        }

        BlockNode multi_decl = new BlockNode(variables);

        return multi_decl;
   }

    public AST FunctionDecl(string name) {
        //string name = this.current_token.val;
        //this.Eat(Tokens.ID);
        BlockNode args = this.Arguments();

        if (this.current_token.type == Tokens.ASSIGN) {
            this.Eat(Tokens.ASSIGN);
            // args are variables so we can use them here too
            return new FunctionDeclaration(name, args, this.Expr());
        }
        // XXX normal fun
        return null;
    }

    public Conditional ConditionalStmt() {
        this.Eat(Tokens.IF);

        AST hypothesis = this.Expr();

        this.Eat(Tokens.THEN);

        AST thesis = this.Expr();

        this.Eat(Tokens.ELSE);

        AST antithesis = this.Expr();

        return new Conditional(
            hypothesis,
            thesis,
            antithesis
        );
    }

    public AST GetSequence() {        
        if (this.current_token.type == Tokens.SEQUENCE_END) {
            this.Eat(Tokens.SEQUENCE_END);
            // terms start == end => always undefined
            return new SequenceLiteral(new Terms(0, 0));
        }
        AST first = this.Expr();
        AST last = first;
        string Type = first.Type;
        List<AST> items = new List<AST>{first};

        while (this.current_token.type != Tokens.SEQUENCE_END && this.current_token.type != Tokens.EOF && this.current_token.type != Tokens.DOT) {
            // finite
            this.Eat(Tokens.COMMA);
            AST item = this.Expr();
            if (item.Type != Type || item.Type == "DYNAMIC" || first.Type == "DYNAMIC") {
                this.Error(
                    new TypeError(
                        $"Inconsistent types of elements for secuence. Expected {Type} found {item.Type}"
                    )
                );
            }
            items.Add(item);
        }

        // exited the loop because there were no COMMAs
        // infinite
        if (this.current_token.type == Tokens.DOT) {
            if (first != last) {
                this.Error(new SyntaxError("Too many starting points for sequence. Expected one element (found many)"));
            }
            if (Type != AST<object>.INTEGER) {
                this.Error(new TypeError($"Can not make an infinite sequence of {Type} (try using integers literals)"));
            }
            // first == last
            this.Eat(Tokens.DOT);
            this.Eat(Tokens.DOT);
            this.Eat(Tokens.DOT);

            int start = first.Eval(null);
            int? end;
            if (this.current_token.type == Tokens.SEQUENCE_END) {
                end = null;
            }
            else {
                if (this.current_token.type != Tokens.INTEGER) {
                    this.Error(new TypeError($"Range end must be an integer. Found {this.current_token.type}"));
                }
                end = this.LiteralNode().Eval(null);
            }

            Terms term_fin = new Terms(start, end);

            this.Eat(Tokens.SEQUENCE_END);

            return new SequenceLiteral(term_fin);
        }

        // finite 
        // we have the items at "items"

        Terms term = new Terms(items);

        this.Eat(Tokens.SEQUENCE_END);

        return new SequenceLiteral(term);
    }

    public AST LiteralNode() {
        Token token = this.current_token;
        if (token.type == Tokens.STRING) {
            this.Eat(Tokens.STRING);
            return new StringLiteral(token.val);
        }
        else if (token.type == Tokens.INTEGER) {
            this.Eat(Tokens.INTEGER);
            return new IntLiteral(Convert.ToInt32(token.val));
        }
        else if (token.type == Tokens.FLOAT) {
            this.Eat(Tokens.FLOAT);
            return new FloatLiteral((float) Math.Round(Convert.ToSingle(token.val), 4));
        }
        else if (token.type == Tokens.SEQUENCE_START) {
            this.Eat(Tokens.SEQUENCE_START);
            return this.GetSequence();
        }

        // else
        this.Error(new SyntaxError($"Invalid literal {token.val} {token.type}"));
        return null;
    }

    public AST Term() {
        AST node = this.Factor();

        while (Lexer.OPERATIONS.Contains(this.current_token.type)) {
            Token token = this.current_token;
            Type ast = null;
            if (token.type == Tokens.MULT) {
                this.Eat(Tokens.MULT);
                ast = typeof(Mult);
            }
            else if (token.type == Tokens.DIV) {
                this.Eat(Tokens.DIV);
                ast = typeof(Division);
            }
            else if (token.type == Tokens.MODULO) {
                this.Eat(Tokens.MODULO);
                ast = typeof(Modulo);
            }
            else if (token.type == Tokens.EXP) {
                this.Eat(Tokens.EXP);
                ast = typeof(Exp);
            }

            AST[] args = new AST[] {node, this.Factor()};

            node = (AST) Activator.CreateInstance(
              type: ast,
              args: args
            );
        }

        return node;
    }

    public AST Factor() {
        Token token = this.current_token;
        AST node = null;

        // check unary first
        if (token.type == Tokens.MINUS) {
            this.Eat(Tokens.MINUS);
            node = new ChangeSign(this.Term());
        }
        else if (token.type == Tokens.NOT) {
            this.Eat(Tokens.NOT);
            node = new Negate(this.Term());
        }
        else if (Lexer.LITERALS.Contains(token.type)) {
            node = this.LiteralNode();
        }
        else if (token.type == Tokens.LET) {
            node = this.Letin();
        }
        else if (token.type == Tokens.LPAREN) {
            this.Eat(Tokens.LPAREN);
            node = this.Expr();
            this.Eat(Tokens.RPAREN);
        }
        else if (token.type == Tokens.IF) {
            node = this.ConditionalStmt();
        }
        else if (token.type == Tokens.ID) {
            node = this.Namespace();
        }
        else {
            this.Error(new SyntaxError($"Missing expression before {token.val}"));
        }

        return node;
    }

    public AST Expr() {
        AST node = this.Term();

        while (new HashSet<string>{Tokens.PLUS, Tokens.MINUS}.Union(Lexer.CONDITIONALS).Contains(this.current_token.type)) {
            Token token = this.current_token;
            Type ast = null;
            if (token.type == Tokens.PLUS) {
                this.Eat(Tokens.PLUS);
                ast = typeof(Sum);
            }
            else if (token.type == Tokens.MINUS) {
                this.Eat(Tokens.MINUS);
                ast = typeof(Substraction);
            }
            else if (token.type == Tokens.EQUALS) {
                this.Eat(Tokens.EQUALS);
                ast = typeof(Equals);
            }
            else if (token.type == Tokens.HIGHER) {
                this.Eat(Tokens.HIGHER);
                ast = typeof(Higher);
            }
            else if (token.type == Tokens.LOWER) {
                this.Eat(Tokens.LOWER);
                ast = typeof(Lower);
            }
            else {
                this.Error(new SyntaxError($"Unknown operand: {token.type}"));
            }

            node = (AST) Activator.CreateInstance(ast, args: new Object[] {node, this.Factor()});
        }
        return node;
    }

    public int Guess() {
        // give an (educated) guess of what kind of tokens are coming next and how they should be parsed

        var token = this.current_token.type;
        // 2 <=> declaration
        // if there are not other tokens but these two
        int count_ids = 0;
        int _let = 0;
        int others = 0;
        int counter = 0;

        while (token != Tokens.END && token != Tokens.EOF) {
            if (token == Tokens.ID) {
                count_ids += 1;
            }
            else if (token == Tokens.ASSIGN) {
                // could be let
                if (_let == 0) {
                    return BestGuess.Declaration;
                }
                // let in the left side <=> expr
                return BestGuess.Expression;
            }
            else if (token == Tokens.LET) {
                _let += 1;
            }
            else {
                others += 1;
            }

            token = this.Peek(counter++).type;
        }

        if (count_ids == 2 && others == 0) {
            return BestGuess.Declaration;
        }
        return BestGuess.Expression;
    }

    protected AST ParseNode() {
        AST node = null;

        int guessed = Guess();

        if (guessed == BestGuess.Expression) {
            node = this.Expr();
        }
        else if (guessed == BestGuess.Declaration) {
            node = this.Declaration();
        }

        if (this.current_token.type != Tokens.END) {
            this.Error(new SyntaxError("Expected ';'"));
        }

        if (!(node is null)) {
            Exception? exc = node.Check();
            if (!(exc is null)) {
                this.Error(exc);
            }
        }

        return node;
    }

    public AST Parse() {
        List<AST> nodes = new List<AST>();
        AST node = null;

        while (this.current_token.type != Tokens.EOF) {
            node = this.ParseNode();
            this.Eat(Tokens.END);
            nodes.Add(node);
        }

        return new BlockNode(nodes);
    }
}

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
        "print",
        new BlockNode(new List<AST>{new Variable("val")}),
        new PrintBlockNode(new Variable("val"))
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
        "cos",
        new BlockNode(new List<AST>{new Variable("val")}),
        new CosBlockNode(new Variable("val"))
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
        "sin",
        new BlockNode(new List<AST>{new Variable("val")}),
        new SinBlockNode(new Variable("val"))
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
        "log",
        new BlockNode(new List<AST>{new Variable("val")}),
        new LogBlockNode(new Variable("val"))
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

public class Interpreter {
    public Context GLOBAL_SCOPE = new Context();
    public Parser parser;
    public AST _tree;
    public BlockNode BUILTINS = new BlockNode(
        new List<AST>{
            new Print(), new Cos(), new Sin(), new Log(), new True(), new False()
        }
    );

    public Interpreter(Parser parser) {
        this.parser = parser;
        this._tree = null;

        BUILTINS.Eval(this.GLOBAL_SCOPE);
    }

    public AST tree {
        get {
            if (this._tree is null) {
                this._tree = this.parser.Parse();
            }
            return this._tree;
        }
        set {}
    }

    public dynamic Interpret() {
        AST tree = this.parser.Parse();
        if (tree is null) {
            return "";
        }
        var eval = tree.Eval(this.GLOBAL_SCOPE);
        if (eval is null) {
            return "";
        }
        return eval;
    }
}
