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


namespace Interpreter
{
    public static class Utils//Encapsula métodos auxiliares
    {
        public static Stack<Brush> COLORS = new Stack<Brush>(new Brush[]{Brushes.Black});

        public static Dictionary<string,dynamic>[] GetIntersectionPoints(Geometry g1, Geometry g2)//Hallar intersección entre dos Geometrys
        {
            Geometry og1 = g1.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));
            Geometry og2 = g2.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));

            CombinedGeometry cg = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);

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

                default: MessageBox.Show("The color selected is not a valid color brush.");break;
            }

        }
        public static void RestoreColor()//go backward de colors
        {
            if (COLORS.Count > 1)
            {
                COLORS.Pop();
            }
        }
        public static IEnumerable<Point> Points(Geometry figure)//puntos aleatorios de figure
        {
            if (figure is LineGeometry line)
            {
                double m = (line.EndPoint.Y - line.StartPoint.Y) / (line.EndPoint.X - line.StartPoint.X); // Calcula la pendiente
                double b = line.StartPoint.Y - (m * line.StartPoint.X); // Calcula el intercepto
               
                    for (int i = 0; i < 10; i++)
                    {
                    double x = Random.Shared.NextDouble() * 10; // Genera un número aleatorio entre el inicio y el fin de la línea
                        double y = m * x + b;
                        yield return new Point(x, y);
                    }               
            }
            else if (figure is PathGeometry arc)
            {
                //Es un arco
            }
            else if (figure is EllipseGeometry circle) //Es un circulo
            {
                if (circle.RadiusX == 4.99)//Descartar que sea la representación geométrica de un punto
                {
                    yield return circle.Center;
                }
                else 
                { 
                   for (int i = 0; i < 10; i++)//Puntos aleatorios de la circunferencia
                   {
                        double angle = Random.Shared.NextDouble() * 2 * Math.PI; // Genera un ángulo aleatorio entre 0 y 2π
                        double x = circle.Center.X + circle.RadiusX * Math.Cos(angle); // Calcula la coordenada x del punto
                        double y = circle.Center.Y + circle.RadiusX * Math.Sin(angle); // Calcula la coordenada y del punto
                        yield return new Point(x, y);
                   }
                }
            }
           
        }
        public static double[] Randoms()//Puntos aleatorios entre cero y uno 
        {
            int size = Random.Shared.Next(2,10);
            double[] result = new double[size];

            int c = 0;
            while (c < size)
            {
                result[c] = Random.Shared.NextDouble();
                c++;
            }
            return result;
        }

        public static string SerialPath() {
            DirectoryInfo BASE_DIR = new DirectoryInfo(
                Assembly.GetAssembly(typeof (_Interpreter)).Location
            ).Parent.Parent.Parent.Parent.Parent;

            return System.IO.Path.Join(BASE_DIR.ToString(), "Serials");
        }

        public static string SerialFile(string filename) {
            return System.IO.Path.Join(SerialPath(), $"{filename}.xaml");
        }

        public static string[] SerialFiles() {
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
            // Deserializa y añade cada Path al Canvas
            try {
                foreach (string filePath in SerialFiles())
                {
                    var xaml = System.IO.File.ReadAllText(filePath);
                    System.Windows.Shapes.Path path = (System.Windows.Shapes.Path)XamlReader.Parse(xaml);
                    myCanvas.Children.Add(path);
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
            }
        }
        public static void ClearSerials()//eliminar los archivos de la anterior compilación
        {
            foreach (var filepath in SerialFiles())
            {
                System.IO.File.Delete(filepath);
            }
        }
        public static Geometry BuildGeometry(Dictionary<string,dynamic> figure)
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
                        Dictionary<string, dynamic> centro = arc["center"];
                        Dictionary<string, dynamic> coord = centro["params"];
                        var center = new Point(coord["x"], coord["y"]);
                        Dictionary<string, dynamic> p2 = arc["p2"];
                        Dictionary<string, dynamic> coordP2 = p2["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        Dictionary<string, dynamic> p3 = arc["p3"];
                        Dictionary<string, dynamic> coordP3 = p3["params"];
                        var punto3 = new Point(coordP3["x"], coordP3["y"]);
                        float measure = arc["measure"];
                    }; break;
            }
            throw new Exception("These objects not can be intersected");
        }
    }
}
