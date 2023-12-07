using System.Text;
using System.Reflection;

using Interpreter.Figures;

namespace Interpreter;

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
    public static string POINT = "POINT";
    public static string LINE = "LINE";
    public static string SEGMENT = "SEGMENT";
    public static string RAY = "RAY";
    public static string CIRCLE = "CIRCLE";
    public static string ARC = "ARC";

    public static HashSet<string> FloatOp = new HashSet<string>{FLOAT, INTEGER};
    public static HashSet<string> BoolOp = new HashSet<string>{FLOAT, INTEGER, BOOL, STRING};
    public static HashSet<string> StrOp = new HashSet<string>{STRING};
    public static HashSet<string> SeqOp = new HashSet<string>{SEQUENCE};

    public static Dictionary<string, HashSet<string>> Compatible = new Dictionary<string, HashSet<string>>{
        {FLOAT, FloatOp},
        {INTEGER, FloatOp},
        {BOOL, BoolOp},
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
        {DYNAMIC, typeof(object)},
        {SEQUENCE, typeof(Terms)},
        {POINT, typeof(Figures.Point)},
        {LINE, typeof(Figures.Line)},
        {SEGMENT, typeof(Figures.Segment)},
        {RAY, typeof(Figures.Ray)},
        {ARC, typeof(Figures.Arc)}
    };
    public static Dictionary<Type, string> RevTypes = new Dictionary<Type, string>{
        {typeof(string), STRING},
        {typeof(int), INTEGER},
        {typeof(float), FLOAT},
        {typeof(bool), BOOL},
        {typeof(object), DYNAMIC},
        {typeof(Terms), SEQUENCE},
        {typeof(SequenceLiteral), SEQUENCE},
        {typeof(Figures.Point), POINT},
        {typeof(Figures.Line), LINE},
        {typeof(Figures.Segment), SEGMENT},
        {typeof(Figures.Ray), RAY},
        {typeof(Figures.Circle), CIRCLE},
        {typeof(Figures.Arc), ARC}
    };

    public static string ToStr() {
        return RevTypes[typeof(T)];
    }

    public static string ToStr(Type type) {
        return RevTypes[type];
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

    public override Exception Check() {
        // propagate type checking
        return this.expression.Check();
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

// always dynamic; a block of ASTs doesn't have a static type
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

    public IEnumerator<AST> GetEnumerator() {
        return this.blocks.GetEnumerator();
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

    public override string Type {
        get {
            return this.body.Type;
        }
    }

    public FunctionDeclaration(string name, BlockNode args=null, AST body=null, int param_count=1) {
        this.name = name;
        // defaults for builtins
        if (args is null && body is null) {
            var variables = new List<AST>();
            for (int i = 0; i < param_count; i++) {
                // a, b, ...
                char var_name = (char) (97 + i);
                variables.Add(new Variable(var_name.ToString()));
            }
            this.args = new BlockNode(variables);

            string qualname = "Interpreter." + this.GetType().Name + "BlockNode";
            Type type = System.Type.GetType(qualname);
            if (type is null) {
                throw new Exception($"{qualname} doesn't exist under this namespace");
            }

            this.body = (AST) Activator.CreateInstance(
                  type: type,
                  // we need to convert because the compilator complains otherwise
                  args: variables.ToArray()
            );
        }
        else {
            this.args = args;
            this.body = body;
        }
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
            catch (System.ArgumentOutOfRangeException) {
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

        var exc = new List<Exception>{
            this.hypothesis.Check(),
            this.thesis.Check(),
            this.antithesis.Check(),
        };
        foreach(Exception item in exc) {
            if (!(item is null)) {
                return item;
            }
        }

        bool dynamic_expr = this.thesis.Type == AST<object>.DYNAMIC || this.antithesis.Type == AST<object>.DYNAMIC;
        bool right_type = AST<object>.Compatible[this.thesis.Type].Contains(this.antithesis.Type);

        if (dynamic_expr || right_type) {
            return null;
        }

        string msg = $"Type mismatch inside Conditional statement : {this.thesis.Type} != {this.antithesis.Type}";
        return new TypeError(msg);
    }
}

