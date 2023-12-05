namespace Interpreter;

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

