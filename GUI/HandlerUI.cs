using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1
{
    public static class HandlerUI
    {
        public static Dictionary<string, Geometry> DrawedFigures = new Dictionary<string, Geometry>();

        public static string Code{get; set;}

        public static int Measure(Point p1 , Point p2)//medida entre dos puntos 
        {
            return Utils.Measure(p1, p2);
        }
        public static Point[] Intersection(Geometry fig1,Geometry fig2)//intersect
        {
            return Utils.GetIntersectionPoints(fig1, fig2);
        }
 
    }
}
