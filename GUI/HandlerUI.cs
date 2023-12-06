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
    public class HandlerUI
    {
        public Dictionary<int, Geometry> DrawedFigures = new Dictionary<int, Geometry>();
        public HandlerUI(string text)
        {
            Code = text;
        }
        public string Code { get; set; }

        public static int Measure(Point p1, Point p2)//medida entre dos puntos 
        {
            return Utils.Measure(p1, p2);
        }
        public static Point[] Intersection(Geometry fig1, Geometry fig2)//intersect
        {
            return Utils.GetIntersectionPoints(fig1, fig2);
        }
        #region Envío de órdenes de dibujo y parámetros normalizados al Drawer
        public static void Draw(Dictionary<string, dynamic> figure)
        {
            string type = figure.Keys.First();
            switch (type)
            {
            }
        }
        #endregion

        #region HandlerUI.GetFigure(Envío de parámetros aleatorios al parser para figuras sin parámetros)
        public static Dictionary<string, Tuple<int, int>> GetPoint() 
        {
              Dictionary<string,Tuple<int,int>> result = new Dictionary<string, Tuple<int, int>>();
              var xAndy=new Tuple<int,int>(Random.Shared.Next(0, 700), Random.Shared.Next(0,800));
            result["Point"]=xAndy;
            return result;
        }
        public static Dictionary<string, Tuple<int, int>> GetLine()
        {
            Dictionary<string, Tuple<int, int>> result = new Dictionary<string, Tuple<int, int>>();
            GetPoint().TryGetValue("Point", out Tuple<int, int> firstCoord);
            result["LineStartPoint"] = firstCoord;
            GetPoint().TryGetValue("Point", out Tuple<int, int> secondCoord);
            result["LineEndPoint"] = secondCoord;

            return result;
        }
        #endregion
    }
}
