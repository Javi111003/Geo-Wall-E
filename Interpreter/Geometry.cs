// geometric locations
namespace Interpreter;

public class DrawParams : Dictionary<string, dynamic> {
}

interface Drawable {
    public DrawParams GetDrawParams();
}

// binary operation because we already have type checking defined for it
// consider it equivalent same as "+(a, b)"
class GenDeclBlockNode<TIn, TOut> : BinaryOperation<TIn, TOut> {

    public GenDeclBlockNode(AST p1, AST p2) : base(p1, p2) {}

    public override TOut Operation(TIn p1, TIn p2) {
        Type type = typeof(TOut);
        return (TOut) Activator.CreateInstance(
                  type: type,
                  // we need to convert because the compilator complains otherwise
                  args: new List<dynamic>{p1, p2}.ToArray()
        );
    }
}

// this is not a literal
// notice we don't need to make another instance
// just to declare a new type of literal
//
// StringLiteral and friends were just aliases
// Point
public class Point : Drawable {

    float x;
    float y;

    public Point(float x, float y) {
        this.x = x;
        this.y = y;
    }

    public DrawParams GetDrawParams() {
        return new DrawParams {
            {"type", this.GetType().Name.ToLower()},
            {"params", new Dictionary<string, float>{{"x", this.x}, {"y", this.y}}}
        };
    }

    public override string ToString() {
        return $"<{this.GetType().Name}({this.x}, {this.y})>";
    }
}

class PointDeclBlockNode : GenDeclBlockNode<float, Point> {

    public PointDeclBlockNode(AST x, AST y) : base(x, y) {}
}

class PointDecl : FunctionDeclaration {

    public PointDecl() : base(
        "point",
        param_count:2
    ) {}
}

// line
public class Line : Drawable {
    Point p1;
    Point p2;

    public Line(Point p1, Point p2) {
        this.p1 = p1;
        this.p2 = p2;
    }

    public DrawParams GetDrawParams() {
        return new DrawParams {
            {"type", this.GetType().Name.ToLower()},
            {"params", new Dictionary<string, Point>{
                {"p1", this.p1},
                {"p2", this.p2}
            }}
        };
    }

    public override string ToString() {
        return $"<{this.GetType().Name}({this.p1}, {this.p2})>";
    }
}

class LineDeclBlockNode : GenDeclBlockNode<Point, Line> {
    public LineDeclBlockNode(AST p1, AST p2) : base(p1, p2) {}
}

class LineDecl : FunctionDeclaration {

    public LineDecl() : base(
        "line",
        param_count:2
    ) {}
}

// it's the same thing for us
// segment
public class Segment : Line {

    public Segment(Point p1, Point p2) : base(p1, p2) {}
}

class SegmentDeclBlockNode : GenDeclBlockNode<Point, Segment> {

    public SegmentDeclBlockNode(AST p1, AST p2) : base(p1, p2) {}
}

class SegmentDecl : FunctionDeclaration {

    public SegmentDecl() : base(
        "segment",
        param_count:2
    ) {}
}

// ray
public class Ray : Line {

    public Ray(Point p1, Point p2) : base(p1, p2) {}
}

class RayDeclBlockNode : GenDeclBlockNode<Point, Ray> {

    public RayDeclBlockNode(AST p1, AST p2) : base(p1, p2) {}
}

class RayDecl : FunctionDeclaration {

    public RayDecl() : base(
        "ray",
        param_count:2
    ) {}
}

// circle
public class Circle : Drawable {
    Point center;
    float radius;

    public Circle(Point center, float radius) {
        this.center = center;
        this.radius = radius;
    }

    public DrawParams GetDrawParams() {
        return new DrawParams {
            {"type", this.GetType().Name.ToLower()},
            {"params", new Dictionary<string, dynamic>{{"center", this.center}, {"radius", this.radius}}}
        };
    }

    public override string ToString() {
        return $"<{this.GetType().Name}({this.center}, {this.radius})>";
    }
}

class CircleDeclBlockNode : GenDeclBlockNode<object, Circle> {

    public CircleDeclBlockNode(AST center, AST radius) : base(center, radius) {}
}

class CircleDecl : FunctionDeclaration {

    public CircleDecl() : base(
        "circle",
        param_count:2
    ) {}
}

// arc
// annoying because it has 4 params
public class Arc : Drawable {
    // i still don't understand what did they mean with these params
    // but it's only used in a dummy test so i guess it's not important
    Point center;
    Point p2;
    Point p3;
    float measure;

    public Arc(Point center, Point p2, Point p3, float measure) {
        this.center = center;
        this.p2 = p2;
        this.p3 = p3;
        this.measure = measure;
    }

    public DrawParams GetDrawParams() {
        return new DrawParams {
            {"type", this.GetType().Name.ToLower()},
            {"params", new Dictionary<string, dynamic>{
                {"center", this.center},
                {"p2", this.p2},
                {"p3", this.p3},
                {"measure", this.measure}
            }}
        };
    }

    public override string ToString() {
        return $"<{this.GetType().Name}({this.center}, {this.p2}, {this.p3}, {this.measure})>";
    }
}

class ArcDeclBlockNode : AST<Arc> {

    AST center, p2, p3, measure;

    public ArcDeclBlockNode(
        AST center,
        AST p2,
        AST p3,
        AST measure
    ) : base(AST<Arc>.ToStr()) {
        this.center = center;
        this.p2 = p2;
        this.p3 = p3;
        this.measure = measure;
    }

    public override dynamic Eval(Context ctx) {
        return new Arc(
            this.center.Eval(ctx),
            this.p2.Eval(ctx),
            this.p3.Eval(ctx),
            this.measure.Eval(ctx)
        );
    }

    // i won't bother with implementing Check()
}

class ArcDecl : FunctionDeclaration {

    public ArcDecl() : base(
        "arc",
        param_count:4
    ) {}
}

// intersect
class IntersectDeclBlockNode : GenDeclBlockNode<Drawable, SequenceLiteral> {

    public IntersectDeclBlockNode(AST f1, AST f2) : base(f1, f2) {}

    public override SequenceLiteral Operation(Drawable f1, Drawable f2) {
        // XXX call handler
        return new SequenceLiteral(
            new Terms(
                // static typing and its consequences have been a disaster for the human race
                (List<AST>) new List<AST>{
                    new Literal<Point>(new Point(0, 0))
                }
            )
         );
    }
}

class IntersectDecl : FunctionDeclaration {

    public IntersectDecl() : base(
        "intersect",
        param_count:2
    ) {}
}

// Utils
class CountDeclBlockNode : UnaryOperation<SequenceLiteral, int> {

    public CountDeclBlockNode(AST seq) : base(seq) {}

    public override int Operation(SequenceLiteral seq) {
        return seq.Val().Count();
    }
}

class CountDecl : FunctionDeclaration {

    public CountDecl() : base(
        "count",
        param_count:1
    ) {}
}

class RandomsDeclBlockNode : AST<SequenceLiteral> {

    public RandomsDeclBlockNode() : base(AST<SequenceLiteral>.ToStr()) {}

    public override dynamic Eval(Context ctx) {
        Random rand = new Random();
        var ls = new List<AST>();
        for(int i = 0; i < 100; i++) {
            ls.Add((AST) new FloatLiteral((float) 1/rand.Next(0, 1000)));
        }
        return new SequenceLiteral(new Terms(ls));
    }
}

class RandomsDecl : FunctionDeclaration {

    public RandomsDecl() : base(
        "randoms",
        param_count:0
    ) {}
}


class PointsDeclBlockNode : UnaryOperation<Drawable, SequenceLiteral> {

    public PointsDeclBlockNode(AST f1) : base(f1) {}

    public override SequenceLiteral Operation(Drawable f1) {
        // XXX call handler
        return new SequenceLiteral(
            new Terms(
                // static typing and its consequences have been a disaster for the human race
                (List<AST>) new List<AST>{
                    new Literal<Point>(new Point(1, 1))
                }
            )
         );
    }
}

class PointsDecl : FunctionDeclaration {

    public PointsDecl() : base(
        "points",
        param_count:1
    ) {}
}

class SamplesDeclBlockNode : AST<SequenceLiteral> {

    public SamplesDeclBlockNode() : base(AST<SequenceLiteral>.ToStr()) {}

    public override dynamic Eval(Context ctx) {
        Random rand = new Random();
        var ls = new List<AST>();
        for(int i = 0; i < 100; i++) {
            ls.Add((AST) new Literal<Point>(new Point(0,0)));
        }
        return new SequenceLiteral(new Terms(ls));
    }
}

class SamplesDecl : FunctionDeclaration {

    public SamplesDecl() : base(
        "samples",
        param_count:0
    ) {}
}
