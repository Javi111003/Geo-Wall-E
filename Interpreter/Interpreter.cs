namespace Interpreter;


public class Interpreter {
    public Context GLOBAL_SCOPE = new Context();
    public Parser parser;
    public AST _tree;
    public BlockNode BUILTINS = new BlockNode(
        new List<AST>{
            new Print(),
            new Cos(),
            new Sin(),
            new Log(),
            new True(),
            new False(),
            new PointDecl(),
            new LineDecl(),
            new SegmentDecl(),
            new RayDecl(),
            new CircleDecl(),
            new ArcDecl(),
            new IntersectDecl()
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
