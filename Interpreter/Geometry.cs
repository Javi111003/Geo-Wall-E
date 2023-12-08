// geometric locations
using Interpreter;

namespace Interpreter {

    interface Drawable {
        public Dictionary<string, dynamic> GetDrawParams();
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

    namespace Figures {
        public class Point : Drawable {

            float x;
            float y;

            public Point(float x, float y) {
                this.x = x;
                this.y = y;
            }

            public Point(Dictionary<string, dynamic> metadata) {
                string name = this.GetType().Name.ToLower();
                if (metadata["type"] != name) {
                     throw new Exception($"Expected type {name} found {metadata["type"]}");
                }
                var param = metadata["params"];
                this.x = param["x"];
                this.y = param["y"];
            }

            public Dictionary<string, dynamic> GetDrawParams() {
                return new Dictionary<string, dynamic> {
                    {"type", this.GetType().Name.ToLower()},
                    {"params", new Dictionary<string, float>{{"x", this.x}, {"y", this.y}}}
                };
            }

            public override string ToString() {
                return $"<{this.GetType().Name}({this.x}, {this.y})>";
            }
        }

        public class Line : Drawable {
            Point p1;
            Point p2;

            public Line(Point p1, Point p2) {
                this.p1 = p1;
                this.p2 = p2;
            }

            public Line(Dictionary<string, dynamic> metadata) {
                string name = this.GetType().Name.ToLower();
                if (metadata["type"] != name) {
                     throw new Exception($"Expected type {name} found {metadata["type"]}");
                }
                var param = metadata["params"];
                this.p1 = new Point(param["p1"]);
                this.p2 = new Point(param["p2"]);
            }

            public Dictionary<string, dynamic> GetDrawParams() {
                return new Dictionary<string, dynamic> {
                    {"type", this.GetType().Name.ToLower()},
                    {"params", new Dictionary<string, dynamic>{
                        {"p1", this.p1.GetDrawParams()},
                        {"p2", this.p2.GetDrawParams()}
                    }}
                };
            }

            public override string ToString() {
                return $"<{this.GetType().Name}({this.p1}, {this.p2})>";
            }
        }

        public class Segment : Line {

            public Segment(Point p1, Point p2) : base(p1, p2) {}
            public Segment(Dictionary<string, dynamic> metadata) : base(metadata) {}
        }

        public class Ray : Line {

            public Ray(Point p1, Point p2) : base(p1, p2) {}
            public Ray(Dictionary<string, dynamic> metadata) : base(metadata) {}
        }

        public class Circle : Drawable {
            Point center;
            float radius;

            public Circle(Point center, float radius) {
                this.center = center;
                this.radius = radius;
            }

            public Circle(Dictionary<string, dynamic> metadata) {
                string name = this.GetType().Name.ToLower();
                if (metadata["type"] != name) {
                     throw new Exception($"Expected type {name} found {metadata["type"]}");
                }
                var param = metadata["params"];
                this.center = new Point(param["center"]);
                this.radius =  param["radius"];
            }

            public Dictionary<string, dynamic> GetDrawParams() {
                return new Dictionary<string, dynamic> {
                    {"type", this.GetType().Name.ToLower()},
                    {"params", new Dictionary<string, dynamic>{{"center", this.center.GetDrawParams()}, {"radius", this.radius}}}
                };
            }

            public override string ToString() {
                return $"<{this.GetType().Name}({this.center}, {this.radius})>";
            }
        }

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

            public Arc(Dictionary<string, dynamic> metadata) {
                string name = this.GetType().Name.ToLower();
                if (metadata["type"] != name) {
                     throw new Exception($"Expected type {name} found {metadata["type"]}");
                }
                var param = metadata["params"];
                this.center = new Point(param["center"]);
                this.p2 = new Point(param["p2"]);
                this.p3 = new Point(param["p3"]);
                this.measure =  param["measure"];
            }

            public Dictionary<string, dynamic> GetDrawParams() {
                return new Dictionary<string, dynamic> {
                    {"type", this.GetType().Name.ToLower()},
                    {"params", new Dictionary<string, dynamic>{
                        {"center", this.center.GetDrawParams()},
                        {"p2", this.p2.GetDrawParams()},
                        {"p3", this.p3.GetDrawParams()},
                        {"measure", this.measure}
                    }}
                };
            }

            public override string ToString() {
                return $"<{this.GetType().Name}({this.center}, {this.p2}, {this.p3}, {this.measure})>";
            }
        }
    }

    // block node

    class PointDeclBlockNode : GenDeclBlockNode<float, Figures.Point> {

        public PointDeclBlockNode(AST x, AST y) : base(x, y) {}
    }

    class PointDecl : FunctionDeclaration {

        public PointDecl() : base(
            "point",
            param_count:2
        ) {}
    }

    // line
    class LineDeclBlockNode : GenDeclBlockNode<Figures.Point, Figures.Line> {
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
    class SegmentDeclBlockNode : GenDeclBlockNode<Figures.Point, Figures.Segment> {

        public SegmentDeclBlockNode(AST p1, AST p2) : base(p1, p2) {}
    }

    class SegmentDecl : FunctionDeclaration {

        public SegmentDecl() : base(
            "segment",
            param_count:2
        ) {}
    }

    // ray
    class RayDeclBlockNode : GenDeclBlockNode<Figures.Point, Figures.Ray> {

        public RayDeclBlockNode(AST p1, AST p2) : base(p1, p2) {}
    }

    class RayDecl : FunctionDeclaration {

        public RayDecl() : base(
            "ray",
            param_count:2
        ) {}
    }

    // circle
    class CircleDeclBlockNode : GenDeclBlockNode<object, Figures.Circle> {

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
    class ArcDeclBlockNode : AST<Figures.Arc> {

        AST center, p2, p3, measure;

        public ArcDeclBlockNode(
            AST center,
            AST p2,
            AST p3,
            AST measure
        ) : base(AST<Figures.Arc>.ToStr()) {
            this.center = center;
            this.p2 = p2;
            this.p3 = p3;
            this.measure = measure;
        }

        public override dynamic Eval(Context ctx) {
            return new Figures.Arc(
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
            var ls = new List<AST>();
            foreach(
                Dictionary<string, dynamic> point in HandlerUI.Intersection(f1.GetDrawParams(), f2.GetDrawParams())
            ) {
                ls.Add(new Literal<Figures.Point>(new Figures.Point((Dictionary<string, dynamic>) point)));
            }

            return new SequenceLiteral(
                new Terms(
                    // static typing and its consequences have been a disaster for the human race
                    ls
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

    // draw
    class DrawDeclBlockNode : UnaryOperation<Drawable, object> {
        AST label;

        public DrawDeclBlockNode(AST block, AST label) : base(block) {
            this.label = label;
        }

        public override dynamic Eval(Context ctx) {
            if (this.block.Type == AST<object>.SEQUENCE) {
                SequenceLiteral seq = (SequenceLiteral) this.block;
                foreach(AST ast in seq.Val()) {
                    if (ast.Type == AST<object>.INTEGER) {
                        throw new TypeError("An integer is not drawable");
                    }
                    Drawable fig = (Drawable) ast.Eval(ctx);
                    this.Operation(fig);
                }
            }
            return null;
        }

        public override object Operation(Drawable fig) {
            HandlerUI.Draw(fig.GetDrawParams(), this.label.ToString());
            return null;
        }
    }

    class DrawDecl : FunctionDeclaration {

        public DrawDecl() : base(
            "draw",
            param_count:2
        ) {}
    }

    // measure
    class MeasureDeclBlockNode : GenDeclBlockNode<Drawable, int> {

        public MeasureDeclBlockNode(AST f1, AST f2) : base(f1, f2) {}

        public override int Operation(Drawable f1, Drawable f2) {
            // XXX call handler
            return HandlerUI.Measure(f1.GetDrawParams(), f2.GetDrawParams());
        }
    }

    class MeasureDecl : FunctionDeclaration {

        public MeasureDecl() : base(
            "measure",
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
            var ls = new List<AST>();
            foreach(
                Dictionary<string, dynamic> point in HandlerUI.Points(f1.GetDrawParams())
            ) {
                ls.Add(new Literal<Figures.Point>(new Figures.Point((Dictionary<string, dynamic>) point)));
            }

            return new SequenceLiteral(
                new Terms(
                    ls
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
                ls.Add((AST) new Literal<Figures.Point>(
                    new Figures.Point(
                        rand.Next(0, 1000), rand.Next(0, 100)
                    )
                ));
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

    class ColorDeclBlockNode : UnaryOperation<string, object> {

        public ColorDeclBlockNode(AST f1) : base(f1) {}

        public override object Operation(string f1) {
            HandlerUI.Color(f1);
            return null;
        }
    }

    class ColorDecl : FunctionDeclaration {

        public ColorDecl() : base(
            "color",
            param_count:1
        ) {}
    }

    class RestoreDeclBlockNode : AST {

        public RestoreDeclBlockNode() : base(AST<object>.ToStr()) {}

        public override dynamic Eval(Context ctx) {
            HandlerUI.Restore();
            return null;
        }
    }

    class RestoreDecl : FunctionDeclaration {

        public RestoreDecl() : base(
            "restore",
            param_count:0
        ) {}
    }
}
