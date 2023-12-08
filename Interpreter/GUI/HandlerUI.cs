using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public static class HandlerUI
    {
        public static Dictionary<int, Geometry> DrawedFigures = new Dictionary<int, Geometry>();
        public static string Code { get; set; }

        public static int Measure(Dictionary<string, dynamic> p1, Dictionary<string, dynamic> p2)//medida entre dos puntos 
        {
            // FIXME
            //return Utils.Measure(p1, p2);
            return -1;
        }

        public static IEnumerable<Dictionary<string, dynamic>> Intersection(Dictionary<string, dynamic> fig1, Dictionary<string, dynamic> fig2)//intersect
        {
            // FIXME
            //return Utils.GetIntersectionPoints(fig1, fig2);
            yield return GetPoint();
        }

        public static IEnumerable<Dictionary<string, dynamic>> Points(Dictionary<string, dynamic> fig1) // points from a figure
        {
            // FIXME
            //return Utils.GetIntersectionPoints(fig1, fig2);
            yield return GetPoint();
        }

        public static void Color() // random points
        {
            // FIXME
            //return Utils.GetIntersectionPoints(fig1, fig2);
            return;
        }

        public static void Restore()
        {
            // FIXME
            //return Utils.GetIntersectionPoints(fig1, fig2);
            return;
        }


        #region Envío de órdenes de dibujo y parámetros normalizados al Drawer
        public static void Draw(Dictionary<string, dynamic> figure, string label)
        {
            string type = figure["type"];
            switch (type)
            {
                case "point":
                    {
                        Dictionary<string, float> parametros = figure["params"];
                        Drawer.DrawPoint(new Point(parametros["x"], parametros["y"]));
                    }; break;
                case "line":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, float> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, float> coordP2 = p1["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        Drawer.DrawLine(punto1, punto2);
                    }; break;
                case "segment":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, float> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, float> coordP2 = p1["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        Drawer.DrawSegment(punto1, punto2);
                    }; break;
                case "ray":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, float> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, float> coordP2 = p1["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        Drawer.DrawRay(punto1, punto2);
                    }; break;
                case "circle":
                    {
                        Dictionary<string, dynamic> circle = figure["params"];
                        Dictionary<string, dynamic> center = circle["center"];
                        Dictionary<string, dynamic> coord = center["params"];
                        var centro = new Point(coord["x"], coord["y"]);
                        float radius = circle["radius"];
                        Drawer.DrawCircle(centro, radius);
                    }; break;
                case "arc":
                    {
                        Dictionary<string, dynamic> arc = figure["params"];
                        Dictionary<string, dynamic> centro = arc["center"];
                        Dictionary<string, dynamic> coord = centro["params"];
                        var center = new Point(coord["x"], coord["y"]);
                        Dictionary<string, dynamic> p2 = arc["p2"];
                        Dictionary<string, float> coordP2 = p2["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        Dictionary<string, dynamic> p3 = arc["p3"];
                        Dictionary<string, float> coordP3 = p3["params"];
                        var punto3 = new Point(coordP3["x"], coordP3["y"]);
                        float measure = arc["measure"];
                        Drawer.DrawArc(center, punto2, punto3, measure);
                    }; break;
            }
        }
        #endregion

        #region HandlerUI.GetFigure(Envío de parámetros aleatorios al parser para figuras sin parámetros)
        public static Dictionary<string, dynamic> GetPoint()
        {
            float x = Random.Shared.NextSingle() * 100;
            float y = Random.Shared.NextSingle() * 100;
            return new Dictionary<string, dynamic>() {{ "type", "point" },
            { "params",new Dictionary<string,float>() { {"x",x},{"y",y } } } };

        }
        public static Dictionary<string, dynamic> GetLine()
        {
            return new Dictionary<string, dynamic>() { { "type", "line" }, { "params", new Dictionary<string, dynamic>() { { "p1", GetPoint() }, { "p2", GetPoint() } } } };
        }
        public static Dictionary<string, dynamic> GetSegment()
        {
            var result = GetLine();
            result["type"] = "segment";
            return result;
        }
        public static Dictionary<string, dynamic> GetRay()
        {
            var result = GetLine();
            result["type"] = "ray";
            return result;
        }
        public static Dictionary<string, dynamic> GetCircle()
        {
            return new Dictionary<string, dynamic>() { { "type", "circle" }, { "params", new Dictionary<string, dynamic>() { { "center", GetPoint() }, { "radius", Random.Shared.NextSingle() * 100 } } } };
        }
        public static Dictionary<string, dynamic> GetArc()
        {
            return new Dictionary<string, dynamic>() { { "type", "arc" }, { "params", new Dictionary<string, dynamic>() { { "center", GetPoint() }, { "p2", GetPoint() }, { "p3", GetPoint() }, { "measure", Random.Shared.NextSingle() * 100 } } } };
        }
        #endregion
    }
}
