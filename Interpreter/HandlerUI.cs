﻿using System;
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
        public static string Code { get; set; }

        public static int Measure(Dictionary<string, dynamic> p1, Dictionary<string, dynamic> p2)//medida entre dos puntos 
        {           
            return Utils.Measure(p1, p2);   
        }

        public static IEnumerable<Dictionary<string, dynamic>> Intersection(Dictionary<string, dynamic> fig1, Dictionary<string, dynamic> fig2)//intersect
        {
          
            Geometry geo1= Utils.BuildGeometry(fig1);
            Geometry geo2= Utils.BuildGeometry(fig2);
            if (fig1["type"] == "circle" && (fig2["type"] == "line" || fig2["type"] == "segment" || fig2["type"]=="ray")) 
            {
                return Utils.IntersectLineCircle((EllipseGeometry)geo1,(LineGeometry) geo2);
            }
            else if ((fig1["type"] == "line" || fig1["type"] == "ray" || fig1["type"]=="segment")&& fig2["type"] == "circle")
            {
                return Utils.IntersectLineCircle((EllipseGeometry)geo2, (LineGeometry)geo1);
            }
            else return Utils.GetIntersectionPoints(geo1, geo2);
        }

        public static IEnumerable<Dictionary<string, dynamic>> Points(Dictionary<string, dynamic> fig1) // points from a figure
        {
            var geo = Utils.BuildGeometry(fig1);
            return Utils.Points(geo);
        }

        public static void Color(string color)
        {
            Utils.SelectColor(color);
        }

        public static void Restore()
        {
           Utils.RestoreColor();
        }


        #region Envío de órdenes de dibujo y parámetros normalizados al Drawer
        public static void Draw(Dictionary<string, dynamic> figure, string label)
        {
            string type = figure["type"];
            switch (type)
            {
                case "point":
                    {
                        Dictionary<string, dynamic> parametros = figure["params"];
                        Drawer.DrawPoint(new Point(parametros["x"], parametros["y"]),label);
                    }; break;
                case "line":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, dynamic> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, dynamic> coordP2 = p2["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        Drawer.DrawLine(punto1, punto2, label);
                    }; break;
                case "segment":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, dynamic> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, dynamic> coordP2 = p2["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        Drawer.DrawSegment(punto1, punto2,label);
                    }; break;
                case "ray":
                    {
                        Dictionary<string, dynamic> puntos = figure["params"];
                        Dictionary<string, dynamic> p1 = puntos["p1"];
                        Dictionary<string, dynamic> p2 = puntos["p2"];
                        Dictionary<string, dynamic> coordP1 = p1["params"];
                        var punto1 = new Point(coordP1["x"], coordP1["y"]);
                        Dictionary<string, dynamic> coordP2 = p2["params"];
                        var punto2 = new Point(coordP2["x"], coordP2["y"]);
                        Drawer.DrawRay(punto1, punto2, label);
                    }; break;
                case "circle":
                    {
                        Dictionary<string, dynamic> circle = figure["params"];
                        Dictionary<string, dynamic> center = circle["center"];
                        Dictionary<string, dynamic> coord = center["params"];
                        var centro = new Point(coord["x"], coord["y"]);
                        float radius = circle["radius"];
                        Drawer.DrawCircle(centro, radius,label);
                    }; break;
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
                        Drawer.DrawArc(center, punto2, punto3, measure,label);
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
