using System;
using System.Reflection;
using System.IO;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using System.Reflection.Emit;
using System.Windows.Controls.Primitives;
using Label = System.Windows.Controls.Label;

namespace Interpreter
{
    public static class Utils//Encapsula métodos auxiliares
    {
        public static Stack<Brush> COLORS = new Stack<Brush>(new Brush[] { Brushes.Black });

        public static Dictionary<string, dynamic>[] GetIntersectionPoints(Geometry g1, Geometry g2)//Hallar intersección entre dos Geometrys
        {
            Geometry og1 = g1.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));
            Geometry og2 = g2.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));

            CombinedGeometry cg1 = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);
            CombinedGeometry cg2 = new CombinedGeometry(GeometryCombineMode.Union, og1, og2);
            CombinedGeometry cg3 = new CombinedGeometry(GeometryCombineMode.Intersect, cg1.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0)), cg2.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0)));
            CombinedGeometry cg = new CombinedGeometry(GeometryCombineMode.Union, cg1.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0)), cg3.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0)));

            PathGeometry pg = cg.GetFlattenedPathGeometry();

            Dictionary<string, dynamic>[] result = new Dictionary<string, dynamic>[pg.Figures.Count];
            for (int i = 0; i < pg.Figures.Count; i++)
            {
                Rect fig = new PathGeometry(new PathFigure[] { pg.Figures[i] }).Bounds;
                var punto = new System.Windows.Point(fig.Left + fig.Width / 2.0, fig.Top + fig.Height / 2.0);
                result[i] = new Dictionary<string, dynamic>() { { "type", "point" }, { "params", new Dictionary<string, dynamic>() { { "x", Convert.ToSingle(punto.X) }, { "y", Convert.ToSingle(punto.Y) } } } };
            }
            return result;
        }
        public static Geometry PointToGeometry(Point point)//poder tartar los puntos como figuras geométricas interceptables y dibujables
        {
            EllipseGeometry representation = new EllipseGeometry();
            representation.Center = point;
            representation.RadiusX = 4.99;
            representation.RadiusY = 4.99;

            return representation;
        }
        public static int Measure(Dictionary<string, dynamic> p1, Dictionary<string, dynamic> p2)//distancia entre dos puntos
        {
            Dictionary<string, dynamic> coordP1 = p1["params"];
            Dictionary<string, dynamic> coordP2 = p2["params"];

            int measure = Convert.ToInt32(Math.Sqrt(Math.Pow(coordP2["y"] - coordP1["y"], 2) + Math.Pow(coordP2["x"] - coordP1["x"], 2)));

            return measure;
        }
        public static void SelectColor(string color)
        {
            //Posibles colores
            switch (color)
            {
                case "blue": COLORS.Push(Brushes.Blue); break;
                case "red": COLORS.Push(Brushes.Red); break;
                case "black": COLORS.Push(Brushes.Black); break;
                case "yellow": COLORS.Push(Brushes.Yellow); break;
                case "cyan": COLORS.Push(Brushes.Cyan); break;
                case "gray": COLORS.Push(Brushes.Gray); break;
                case "white": COLORS.Push(Brushes.White); break;
                case "gren": COLORS.Push(Brushes.Green); break;
                case "magenta": COLORS.Push(Brushes.Magenta); break;

                default: MessageBox.Show("The color selected is not a valid color brush."); break;
            }

        }
        public static void RestoreColor()//go backward de colors
        {
            if (COLORS.Count > 1)
            {
                COLORS.Pop();
            }
        }
        public static IEnumerable<Dictionary<string,dynamic>> Points(Geometry figure)//puntos aleatorios de figure
        {
            if (figure is LineGeometry line)
            {
                float m = (float)((line.EndPoint.Y - line.StartPoint.Y) / (line.EndPoint.X - line.StartPoint.X)); // Calcula la pendiente
                float b = (float)(line.StartPoint.Y - (m * line.StartPoint.X)); // Calcula el intercepto

                for (int i = 0; i < 10; i++)
                {
                    float x = Random.Shared.Next((int)line.StartPoint.X, (int)line.EndPoint.X); // Genera un número aleatorio entre el inicio y el fin de la línea
                    float y = m * x + b;
                    yield return new Dictionary<string, dynamic>() { { "type", "point" }, { "params", new Dictionary<string, dynamic>() { { "x", x },{ "y", y } } } };
                }
            }
            else if (figure is PathGeometry arc)
            {
                //Es un arco
                Random random = new Random();

                PathFigure pathFigure = arc.Figures[0];
                ArcSegment arcSegment = (ArcSegment)pathFigure.Segments[0];

                // Calcula el ángulo del arco
                float angle = (float)Math.Atan2(arcSegment.Point.Y - pathFigure.StartPoint.Y, arcSegment.Point.X - pathFigure.StartPoint.X);

                // Genera un ángulo aleatorio dentro del rango del arco
                float randomAngle = (float)random.NextDouble() * angle;

                float x =(float) (pathFigure.StartPoint.X + arcSegment.Size.Width * Math.Cos(randomAngle));
                float y = (float)(pathFigure.StartPoint.Y + arcSegment.Size.Height * Math.Sin(randomAngle));

                yield return new Dictionary<string, dynamic>() { { "type", "point" }, { "params", new Dictionary<string, dynamic>() { { "x", x },{ "y", y } } } }; ;
            }
            else if (figure is EllipseGeometry circle) //Es un circulo
            {
                if (circle.RadiusX == 4.99)//Descartar que sea la representación geométrica de un punto
                {
                    yield return new Dictionary<string, dynamic>() { { "type", "point" }, { "params", new Dictionary<string, dynamic>() { { "x", (float)circle.Center.X },{ "y", (float)circle.Center.Y } } } }; ;
                }
                else
                {
                    for (int i = 0; i < 10; i++)//Puntos aleatorios de la circunferencia
                    {
                        float angle = (float)(Random.Shared.NextDouble() * 2 * Math.PI); // Genera un ángulo aleatorio entre 0 y 2π
                        float x =(float) (circle.Center.X + circle.RadiusX * Math.Cos(angle)); // Calcula la coordenada x del punto
                        float y =(float) (circle.Center.Y + circle.RadiusX * Math.Sin(angle)); // Calcula la coordenada y del punto
                        yield return new Dictionary<string, dynamic>() { { "type", "point" }, { "params", new Dictionary<string, dynamic>() { { "x", x },{ "y", y } } } }; ;
                    }
                }
            }

        }
        public static float[] Randoms()//Puntos aleatorios entre cero y uno 
        {
            int size = Random.Shared.Next(2, 10);
            float[] result = new float[size];

            int c = 0;
            while (c < size)
            {
                result[c] = Random.Shared.NextSingle();
                c++;
            }
            return result;
        }

        public static string SerialPath()
        {
            DirectoryInfo BASE_DIR = new DirectoryInfo(
                Assembly.GetAssembly(typeof(_Interpreter)).Location
            ).Parent.Parent.Parent.Parent.Parent;

            return System.IO.Path.Join(BASE_DIR.ToString(), "Serials");
        }

        public static string SerialFile(string filename)
        {
            return System.IO.Path.Join(SerialPath(), $"{filename}.xaml");
        }

        public static string[] SerialFiles()
        {
            return System.IO.Directory.GetFiles(SerialPath(), "*.xaml");
        }

        public static void SavePath(System.Windows.Shapes.Path path, string fileName)//Serializar la figura para poder ser representada posteriormente
        {
            string filePath = SerialFile(fileName);

            // Crea un nuevo archivo
            using (var file = System.IO.File.Create(filePath)) { }

            // Guarda el objeto Path en el archivo
            var xaml = XamlWriter.Save(path);
            System.IO.File.WriteAllText(filePath, xaml);
        }
        public static void LoadAllPaths(Canvas myCanvas)//deserializar 
        {
            LoadAllLabels(myCanvas);//añade los labels asociados a cada figura
           
            // Deserializa y añade cada Path al Canvas
            try
            {
                foreach (string filePath in SerialFiles())
                {
                    var xaml = System.IO.File.ReadAllText(filePath);
                    System.Windows.Shapes.Path path = (System.Windows.Shapes.Path)XamlReader.Parse(xaml);
                    myCanvas.Children.Add(path);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private static void LoadAllLabels(Canvas myCanvas)//añade los labels en su posicion al canvas 
        {
            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.ScaleY = -1;

            foreach (var pair in Drawer.Labels)
            {
                var label = pair.Key;
                // Aplicar la transformación de rotación al Label 
                label.RenderTransform = scaleTransform;
                //posicionar posteriormente en el canvas
                var posLeft = Drawer.Labels[pair.Key].Item1;
                var posUp = Drawer.Labels[pair.Key].Item2;
                //añadir al canvas 
                myCanvas.Children.Add(pair.Key);
                Canvas.SetLeft(label, posLeft);
                Canvas.SetTop(label, posUp);
            }
        }
        public static void ClearSerials()//eliminar los archivos de la anterior compilación
        {
            foreach (var filepath in SerialFiles())
            {
                System.IO.File.Delete(filepath);
            }
        }
        public static Geometry BuildGeometry(Dictionary<string, dynamic> figure)
        {
            string type = figure["type"];

            switch (type)
            {
                case "point":
                    {
                        Dictionary<string, dynamic> parametros = figure["params"];
                        return Utils.PointToGeometry(new Point(parametros["x"], parametros["y"]));
                    }
                case "line":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, dynamic> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, dynamic> coordP2 = p2["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        var line = new LineGeometry();
                        line.StartPoint = punto1;
                        line.EndPoint = punto2;
                        return line;
                    }
                case "segment":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, dynamic> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, dynamic> coordP2 = p2["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        var line = new LineGeometry();
                        line.StartPoint = punto1;
                        line.EndPoint = punto2;
                        return line;
                    }
                case "ray":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, dynamic> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, dynamic> coordP2 = p2["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        var line = new LineGeometry();
                        line.StartPoint = punto1;
                        line.EndPoint = punto2;
                        return line;
                    }
                case "circle":
                    {
                        Dictionary<string, dynamic> circle = figure["params"];
                        Dictionary<string, dynamic> center = circle["center"];
                        Dictionary<string, dynamic> coord = center["params"];
                        var centro = new Point(coord["x"], coord["y"]);
                        float radius = circle["radius"];
                        var circleGeo = new EllipseGeometry();
                        circleGeo.RadiusX = radius;
                        circleGeo.RadiusY = radius;
                        circleGeo.Center = centro;//centro 
                        return circleGeo;
                    }
                case "arc":
                    {
                        Dictionary<string, dynamic> arc = figure["params"];
                        Dictionary<string, dynamic> center = arc["center"];
                        Dictionary<string, dynamic> coord = center["params"];
                        var centro = new Point(coord["x"], coord["y"]);
                        Dictionary<string, dynamic> p2 = arc["p2"];
                        Dictionary<string, dynamic> coordP2 = p2["params"];
                        var inicio = new Point(coordP2["x"], coordP2["y"]);
                        Dictionary<string, dynamic> p3 = arc["p3"];
                        Dictionary<string, dynamic> coordP3 = p3["params"];
                        var fin = new Point(coordP3["x"], coordP3["y"]);
                        float measure = arc["measure"];
                        double anguloInicio = Math.Atan2(inicio.Y - centro.Y, inicio.X - centro.X) * 180.0 / Math.PI;
                        double anguloFin = Math.Atan2(fin.Y - centro.Y, fin.X - centro.X) * 180.0 / Math.PI;
                        // Crear el arco
                        PathGeometry arco = new PathGeometry();
                        PathFigure pathFigure = new PathFigure();
                        pathFigure.StartPoint = inicio;
                        arco.Figures.Add(pathFigure);
                        ArcSegment arcSegment = new ArcSegment();
                        arcSegment.Point = fin;
                        arcSegment.Size = new Size(measure, measure);
                        arcSegment.IsLargeArc = Math.Abs(anguloFin - anguloInicio) > 180.0;
                        arcSegment.SweepDirection = SweepDirection.Clockwise;
                        pathFigure.Segments.Add(arcSegment);
                        return arco;
                        
                    }
            }
            throw new Exception("These objects not can be intersected");
        }
        public static List<Dictionary<string, dynamic>> IntersectLineCircle(EllipseGeometry ellipse, LineGeometry line)
        {

            float dx = (float)(line.EndPoint.X - line.StartPoint.X);
            float dy = (float)(line.EndPoint.Y - line.StartPoint.Y);

            var a = Math.Pow(dx, 2) + Math.Pow(dy, 2);
            var b = 2 * dx * (line.StartPoint.X - ellipse.Center.X) + 2 * dy * (line.StartPoint.Y - ellipse.Center.Y);
            var c = Math.Pow(line.StartPoint.X - ellipse.Center.X, 2) + Math.Pow(line.StartPoint.Y - ellipse.Center.Y, 2) - Math.Pow(ellipse.RadiusX, 2);

            var D = Math.Pow(b, 2) - 4 * a * c;

            if (D < 0)
            {
                return new List<Dictionary<string, dynamic>>();
            }

            float t1 = (float)((-b + Math.Sqrt(D)) / (2 * a));
            float x1 = (float)(line.StartPoint.X + t1 * dx);
            float y1 = (float)(line.StartPoint.Y + t1 * dy);

            if (D == 0)
            {
                return new List<Dictionary<string, dynamic>>() { new Dictionary<string, dynamic>() { { "type", "point" }, { "params", new Dictionary<string, dynamic>() { { "x", x1 }, { "y", y1 } } } } };
            }

            float t2 = (float)((-b - Math.Sqrt(D)) / (2 * a));
            float x2 = (float)(line.StartPoint.X + t2 * dx);
            float y2 = (float)(line.StartPoint.Y + t2 * dy);

            return new List<Dictionary<string, dynamic>>() { new Dictionary<string, dynamic>() { { "type", "point" }, { "params", new Dictionary<string, dynamic>() { { "x", x1 }, { "y", y1 } } } }, new Dictionary<string, dynamic>() { { "type", "point" }, { "params", new Dictionary<string, dynamic>() { { "x", x2 }, { "y", y2 } } } } };
        }
    }
}
