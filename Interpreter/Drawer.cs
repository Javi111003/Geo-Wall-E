using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;


namespace Interpreter
{
    public static class Drawer
    {
        public static void DrawPoint(Point punto)
        {
            var point = punto;
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.Fill = Utils.COLORS.Peek();

            var representation = Utils.PointToGeometry(point);
            myPath.Data = representation;

            Utils.SavePath(myPath, representation.GetHashCode().ToString());
        }
        public static void DrawSegment(Point p1,Point p2)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.White);

            var line = new LineGeometry();
            line.StartPoint = p1;
            line.EndPoint = p2;
            myPath.Data = line;

            Utils.SavePath(myPath, line.GetHashCode().ToString());
        } 
        public static void DrawLine(Point p1, Point p2)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);
            //recibe los puntos 
            Point punto1 = p1;
            Point punto2 = p2;

            // Calcula la pendiente de la línea
            double m = (punto2.Y - punto1.Y) / (punto2.X - punto1.X);

            // Calcula el intercepto con el eje y de la línea
            double b = punto1.Y - m * punto1.X;

            LineGeometry linea = new LineGeometry();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            // Para el punto inicial (X1, Y1), se usa X1 = -2000 ya que  es donde incia el canvas y luego resolver la ecuación de la línea para Y1
            double lineaX1 = -2000;
            double lineaY1 = m * lineaX1 + b;

            // Para el punto final (X2, Y2) se usa ancho del canvas y luego resolver la ecuación de la línea para Y2
            double lineaX2 = 2000;//MyCanvas.ActualWidth;
            double lineaY2 = m * lineaX2 + b;

            linea.StartPoint = new Point(lineaX1, lineaY1);
            linea.EndPoint = new Point(lineaX2, lineaY2);
            myPath.Data = linea;

            Utils.SavePath(myPath, linea.GetHashCode().ToString());
        }
        public static void DrawRay(Point p1 , Point p2)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);

            //recibe los puntos 
            Point punto1 = p1;
            Point punto2 = p2;

            // Calcula la pendiente de la línea
            double m = (punto2.Y - punto1.Y) / (punto2.X - punto1.X);

            // Calcula el intercepto con el eje y de la línea
            double b = punto1.Y - m * punto1.X;

            LineGeometry linea = new LineGeometry();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            // Para el punto inicial (X1, Y1), se usa X1 y luego resolver la ecuación de la línea para Y1
            double lineaX1 = punto1.X;
            double lineaY1 = m * lineaX1 + b;

            // Para el punto final (X2, Y2) se usa ancho del canvas y luego resolver la ecuación de la línea para Y2
            double lineaX2 = -2000;//MyCanvas.ActualWidth;
            if (p1.X - p2.X < 0)//pendiente negativa 
            {
                lineaX2 = 2000;
            }
            double lineaY2 = m * lineaX2 + b;

            linea.StartPoint = new Point(lineaX1, lineaY1);
            linea.EndPoint = new Point(lineaX2, lineaY2);
            myPath.Data = linea;

            Utils.SavePath(myPath, linea.GetHashCode().ToString());
        }
        public static void DrawCircle(Point center,float radio)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);

            var circle = new EllipseGeometry();
            //mismo radio para ambos parametros para que la excentricidad sea = 1 y resulte en una circunferencia
            circle.RadiusX = radio;
            circle.RadiusY = radio;
            circle.Center = center;//centro 
            myPath.Data = circle;
            Utils.SavePath(myPath, circle.GetHashCode().ToString());
        }
        public static void DrawArc(Point center , Point p2 , Point p3 , float radius)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);

            Point centro = center;
            Point extremo1 = p2;
            Point extremo2 = p3;

            ArcSegment arc1 = new ArcSegment();
            arc1.Size = new Size(radius,radius); // Tamaño del primer arco
            arc1.SweepDirection = SweepDirection.Clockwise; // Dirección del primer arco
            arc1.Point = extremo1; // Punto final del primer arco

            ArcSegment arc2 = new ArcSegment();
            arc2.Size = new Size(radius, radius); // Tamaño del segundo arco
            arc2.SweepDirection = SweepDirection.Clockwise; // Dirección del segundo arco
            arc2.Point = extremo2; // Punto final del segundo arco

            //Añadir cada uno de los arcos
            PathSegmentCollection union = new PathSegmentCollection();
            union.Add(arc1);
            union.Add(arc2);

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = centro; // Punto inicial del arco
            pathFigure.Segments = union;//añadiendo la union al Path

            PathGeometry unionAsGeometry = new PathGeometry();
            unionAsGeometry.Figures.Add(pathFigure);
            myPath.Data = unionAsGeometry;

            Utils.SavePath(myPath, unionAsGeometry.GetHashCode().ToString());

        }
    }
}
