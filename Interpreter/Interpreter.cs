namespace Interpreter;


public class _Interpreter {
    public Context GLOBAL_SCOPE = new Context();
    public Parser parser;
    public AST _tree;
    public static BlockNode BUILTINS = new BlockNode(Parser.BUILTINS);

    public _Interpreter(Parser parser) {
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

    public IEnumerator<dynamic> GetEnumerator() {
        BlockNode tree = this.parser.Parse();
        if (tree is null) {
            yield return "";
            yield break;
        }
        foreach(var ast in tree) {
            yield return ast.Eval(this.GLOBAL_SCOPE);
        }
    }
}
