using System.Text;

namespace Interpreter;

public class Literal<T>: AST<T> {
    protected T val;
    public bool IsSequence;

    public Literal(T val): base(AST<T>.ToStr()){
        this.val = val;
        this.IsSequence = false;
    }

    public T Val() {
        return this.val;
    }

    public override dynamic Eval(Context ctx) {
        return this.val;
    }

    public override string ToString() {
        if (this.val is null) {
            return "";
        }
        return this.val.ToString();
    }

    // no need to override check
}

public class StringLiteral: Literal<string> {

    public StringLiteral(string val): base(val) {}
}

public class IntLiteral : Literal<int> {

    public IntLiteral(int val): base(val) {}
}

public class FloatLiteral : Literal<float> {

    public FloatLiteral(float val): base(val) {}
}

public class BoolLiteral : Literal<bool> {

    public BoolLiteral(bool val): base(val) {}
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

    public int Count() {
        if (this.IsInfinite) {
            return Int32.MaxValue;
        }
        return end - index - 1;
    }

    public override Exception Check() {
        if (this.IsInfinite) {
            // INTEGER
            return null;
        }
        // finite
        // check all types
        //
        // the first check will be true
        // but it could also be DYNAMIC
        string type = this.Type;
        foreach(AST item in this) {
            if (item.Type == AST<object>.DYNAMIC) {
                return null;
            }
            bool right_type = AST<object>.Compatible[type].Contains(item.Type);
            if (!right_type) {
                Exception exc = new TypeError(
                    $"Inconsistent types of elements for secuence. Expected {type} found {item.Type}"
                );
                return exc;
            }
        }

        return null;
    }

    public override string ToString() {
        StringBuilder str = new StringBuilder("{");
        if (!this.IsInfinite) {
            foreach(AST item in this) {
                str.Append(item.ToString() + ", ");
            }
        }
        else {
            str.Append($"{this.index + 1}...");
        }
        str.Append("}");
        return str.ToString();
    }
}

// block node... reimagined
public class SequenceLiteral : Literal<Terms> {

    public override string Type {
        get {
            return this.val.Type;
        }
    }

    public SequenceLiteral(Terms val): base(val) {
        this.IsSequence = true;
    }

    public SequenceLiteral Clone() {
        return new SequenceLiteral(this.val.Clone());
    }

    public override dynamic Eval(Context ctx) {
        if (!this.val.IsInfinite) {
            var new_terms = new List<AST>();
            var tp = this.Type;
            foreach(AST ast in this.val) {
                var lit = new Literal<dynamic>(ast.Eval(ctx));
                lit.Type = tp;
                new_terms.Add(new Literal<dynamic>(ast.Eval(ctx)));
            }
            this.val = new Terms(new_terms);
        }
        return this;
    }

    public override string ToString() {
        return this.val.ToString();
    }

    public static SequenceLiteral operator+(SequenceLiteral x, SequenceLiteral y) {
        var ls = new List<AST>();

        if (x.Val().IsInfinite) {
            return x.Clone();
        }
        else if (y.Val().IsInfinite) {
            return y.Clone();
        }

        foreach(AST ast in x.Val()) {
            ls.Add(ast);
        }
        foreach(AST ast in y.Val()) {
            ls.Add(ast);
        }

        return new SequenceLiteral(new Terms(ls));
    }

    public override Exception Check() {
        return this.val.Check();
    }

    public IEnumerator<AST> GetEnumerator() {
        // convenience method
        return this.val.GetEnumerator();
    }
}

