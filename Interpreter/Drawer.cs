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
        public static Dictionary<Label,Tuple<double,double>> Labels =new();//guarda los labels a ser añadidos en el momento de deserializar
        public static void DrawPoint(Point punto,string label="")
        {
            var point = punto;
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.Fill = Utils.COLORS.Peek();

            var representation = Utils.PointToGeometry(point);//representado como un diminuto círculo para poder ser tratado como un objeto representable 
            myPath.Data = representation;

            Label myLabel = new Label();//Texto a ser añadido al canvas
            myLabel.Content = label;
            myLabel.Background = Brushes.Transparent;
            myLabel.Foreground = Brushes.Black;
            //añadirlo a los labels por representar con la posicion cercana a la figura
            Labels[myLabel] = new Tuple<double, double>(myPath.Data.Bounds.Left + myPath.Data.Bounds.Width / 4 - myLabel.ActualWidth / 4, myPath.Data.Bounds.Top + myPath.Data.Bounds.Height / 4 - myLabel.ActualHeight / 4);

            Utils.SavePath(myPath, representation.GetHashCode().ToString());
        }
        public static void DrawSegment(Point p1,Point p2,string label="")
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.White);

            var line = new LineGeometry();
            line.StartPoint = p1;//puntos que delimitan el segmento y listo
            line.EndPoint = p2;
            myPath.Data = line;

            Label myLabel = new Label();//Texto a ser añadido al canvas
            myLabel.Content = label;
            myLabel.Background = Brushes.Transparent;
            myLabel.Foreground = Brushes.Black;
            //añadirlo a los labels por representar con la posicion cercana a la figura
            Labels[myLabel] = new Tuple<double, double>(myPath.Data.Bounds.Left + myPath.Data.Bounds.Width / 4 - myLabel.ActualWidth / 4, myPath.Data.Bounds.Top + myPath.Data.Bounds.Height / 4 - myLabel.ActualHeight / 4);

            Utils.SavePath(myPath, line.GetHashCode().ToString());
        } 
        public static void DrawLine(Point p1, Point p2, string label = "")
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
            double lineaX1 = -2000;//para dar la impresión de infinitud
            double lineaY1 = m * lineaX1 + b;

            // Para el punto final (X2, Y2) se usa ancho del canvas y luego resolver la ecuación de la línea para Y2
            double lineaX2 = 2000;//MyCanvas.ActualWidth;
            double lineaY2 = m * lineaX2 + b;

            linea.StartPoint = new Point(lineaX1, lineaY1);
            linea.EndPoint = new Point(lineaX2, lineaY2);
            myPath.Data = linea;

            Label myLabel = new Label();//Texto a ser añadido al canvas
            myLabel.Content = label;
            myLabel.Background = Brushes.Transparent;
            myLabel.Foreground = Brushes.Black;
            //añadirlo a los labels por representar con la posicion cercana a la figura
            Labels[myLabel] = new Tuple<double, double>(myPath.Data.Bounds.Left + myPath.Data.Bounds.Width / 4 - myLabel.ActualWidth / 4, myPath.Data.Bounds.Top + myPath.Data.Bounds.Height / 4 - myLabel.ActualHeight / 4);

            Utils.SavePath(myPath, linea.GetHashCode().ToString());
        }
        public static void DrawRay(Point p1 , Point p2, string label = "")
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

            Label myLabel = new Label();//Texto a ser añadido al canvas
            myLabel.Content = label;
            myLabel.Background = Brushes.Transparent;
            myLabel.Foreground = Brushes.Black;
            //añadirlo a los labels por representar con la posicion cercana a la figura
            Labels[myLabel] = new Tuple<double, double>(myPath.Data.Bounds.Left + myPath.Data.Bounds.Width / 4 - myLabel.ActualWidth / 4, myPath.Data.Bounds.Top + myPath.Data.Bounds.Height / 4 - myLabel.ActualHeight / 4);

            Utils.SavePath(myPath, linea.GetHashCode().ToString());
        }
        public static void DrawCircle(Point center,float radio,string label="")
        {
            Path myPath = new Path();
            myPath.Stroke = Utils.COLORS.Peek();
            myPath.StrokeThickness = 2;
            myPath.Fill = new SolidColorBrush(Colors.Transparent);

            var circle = new EllipseGeometry();
            //mismo radio para ambos parámetros para que la excentricidad sea = 1 y resulte en una circunferencia
            circle.RadiusX = radio;
            circle.RadiusY = radio;
            circle.Center = center;//centro 
            myPath.Data = circle;

            Label myLabel = new Label();//Texto a ser añadido al canvas
            myLabel.Content = label;
            myLabel.Background = Brushes.Transparent;
            myLabel.Foreground = Brushes.Black;
            //añadirlo a los labels por representar con la posicion cercana a la figura
            Labels[myLabel] = new Tuple<double, double>(myPath.Data.Bounds.Left + myPath.Data.Bounds.Width / 4 - myLabel.ActualWidth / 4, myPath.Data.Bounds.Top + myPath.Data.Bounds.Height / 4 - myLabel.ActualHeight / 4);

            Utils.SavePath(myPath, circle.GetHashCode().ToString());
        }
        public static void DrawArc(Point centro , Point inicio , Point fin , float radio,string label="")
        {
            //Para determinar el cuadrante
            double anguloInicio = Math.Atan2(inicio.Y - centro.Y, inicio.X - centro.X) * 180.0 / Math.PI;//angulo entre el rayo que pasa por p1 con la recta horizontal que pasa por el centro
            double anguloFin = Math.Atan2(fin.Y - centro.Y, fin.X - centro.X) * 180.0 / Math.PI;//angulo entre el rayo que pasa por p2 y la anterior recta horizontal

            // Crear el arco
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = inicio;
            pathGeometry.Figures.Add(pathFigure);
            ArcSegment arcSegment = new ArcSegment();
            arcSegment.Point = fin;
            arcSegment.Size = new Size(radio, radio);
            arcSegment.IsLargeArc = Math.Abs(anguloFin - anguloInicio) > 180.0;
            arcSegment.SweepDirection = SweepDirection.Clockwise;
            pathFigure.Segments.Add(arcSegment);

            // Dibujar el arco en el canvas
            Path myPath = new Path();
            myPath.Data = pathGeometry;
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
        
            Label myLabel = new Label();//Texto a ser añadido al canvas
            myLabel.Content = label;
            myLabel.Background = Brushes.Transparent;
            myLabel.Foreground = Brushes.Black;
            //añadirlo a los labels por representar con la posicion cercana a la figura
            Labels[myLabel] = new Tuple<double, double>(myPath.Data.Bounds.Left + myPath.Data.Bounds.Width / 4 - myLabel.ActualWidth / 4, myPath.Data.Bounds.Top + myPath.Data.Bounds.Height / 4 - myLabel.ActualHeight / 4);


            Utils.SavePath(myPath, pathGeometry.GetHashCode().ToString());

        }
    }
}
