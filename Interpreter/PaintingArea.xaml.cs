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
using Point = System.Windows.Point;


namespace Interpreter
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
        private void DrawFigures(object sender,RoutedEventArgs e)
        {
            Utils.LoadAllPaths(MyCanvas);
        }
 
        public void ClearCanvas(object sender, RoutedEventArgs e)
        {
            MyCanvas.Children.Clear();
        }
     
    }
    
    
}
