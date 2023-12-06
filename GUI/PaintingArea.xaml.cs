using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para PaintingArea.xaml
    /// </summary>
    public partial class PaintingArea : Window
    {
        public PaintingArea()
        {
            InitializeComponent();
        }
        private void DrawSegment(object sender, RoutedEventArgs e)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.White);

            var line = new LineGeometry();
            line.StartPoint = new Point(50,50);
            line.EndPoint=new Point(100,100);
            myPath.Data = line;

            Utils.SavePath(myPath, "segmento");
        }
        private void DrawPoint(object sender,RoutedEventArgs e)
        {
            //[1] = {5.667755461688932,5005.276715992176}
            var point = new Point(5.667755461688932, 50.276715992176);
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.Fill = Utils.COLORS.Peek();

            var representation = Utils.PointToGeometry(point);
            myPath.Data=representation;

            Utils.SavePath(myPath, "punto");
            Utils.LoadAllPaths(MyCanvas);

        }
        private void DrawCircle(object sender, RoutedEventArgs e)
        {
            
            Path myPath = new Path();
             myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);

            var circle = new EllipseGeometry();
            //mismo radio para ambos parametros para que la excentricidad sea = 1 y resulte en una circunferencia
            circle.RadiusX = 50;
            circle.RadiusY = 50;
            circle.Center = new Point(75,75);//centro 
            myPath.Data= circle;
            Utils.SavePath(myPath, "circulo");

        }
        private void DrawLine(object sender,RoutedEventArgs e)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);
            //recibe los puntos 
            Point punto1 = new Point(50, 50);
            Point punto2 = new Point(100, 100);

            // Calcula la pendiente de la línea
            double m = (punto2.Y - punto1.Y) / (punto2.X - punto1.X);

            // Calcula el intercepto con el eje y de la línea
            double b = punto1.Y - m * punto1.X;

            LineGeometry linea = new LineGeometry();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            // Para el punto inicial (X1, Y1), se usa X1 = 0 y luego resolver la ecuación de la línea para Y1
            double lineaX1 = 0;
            double lineaY1 = m * lineaX1 + b;

            // Para el punto final (X2, Y2) se usa ancho del canvas y luego resolver la ecuación de la línea para Y2
            double lineaX2 = MyCanvas.ActualWidth;
            double lineaY2 = m * lineaX2 + b;

            linea.StartPoint=new Point(lineaX1, lineaY1);
            linea.EndPoint=new Point(lineaX2, lineaY2);
            myPath.Data = linea;

            Utils.SavePath(myPath, "recta");

        }
        private void DrawArc(object sender, RoutedEventArgs e)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);

            Point centro = new Point(50, 50);
            Point extremo1 = new Point(50,60 );
            Point extremo2 = new Point(40, 50);

            ArcSegment arc1 = new ArcSegment();
            arc1.Size = new Size(100, 50); // Tamaño del primer arco
            arc1.SweepDirection = SweepDirection.Clockwise; // Dirección del primer arco
            arc1.Point = extremo1; // Punto final del primer arco

            ArcSegment arc2 = new ArcSegment();
            arc2.Size = new Size(100, 50); // Tamaño del segundo arco
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
            myPath.Data= unionAsGeometry;

            Utils.SavePath(myPath, "arc");
        }
        private void DrawRay(object sender , RoutedEventArgs e)
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);

            //recibe los puntos 
            Point punto1 = new Point(50, 50);
            Point punto2 = new Point(100, 100);

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
            double lineaX2 = MyCanvas.ActualWidth;
            double lineaY2 = m * lineaX2 + b;

            linea.StartPoint = new Point(lineaX1, lineaY1);
            linea.EndPoint = new Point(lineaX2, lineaY2);
            myPath.Data = linea;

            Utils.SavePath(myPath, "ray");
        }
        public void ClearCanvas(object sender, RoutedEventArgs e)
        {
            MyCanvas.Children.Clear();
        }
     
    }
    
    
}
