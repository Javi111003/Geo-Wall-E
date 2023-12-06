using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool STATUS = true;
        public MainWindow()
        {
            InitializeComponent();
            Utils.COLORS=new();Utils.COLORS.Push(Brushes.Black);
            Status.Text = "Estado: Listo";
            Status.Foreground = Brushes.Green;
        }

        private void Build_Click(object sender, RoutedEventArgs e)//Boton para compilar
        {
            HandlerUI handler = new HandlerUI(myTextBox.Text);
            Utils.ClearSerials();
            MessageBox.Show("the app is building");
            if (!STATUS) { Status.Foreground = Brushes.Red; Status.Text = "Estado : Errores pendientes"; }
        }

        //Abrir area de dibujo
        private void Run_Click(object sender, RoutedEventArgs e)//Boton para  dibujar
        {
            PaintingArea paintingArea = new PaintingArea();
            paintingArea.Show();
        }

        #region Lógica de Menú Archivo
        private void NewFile(object sender, RoutedEventArgs e)
        {
            // Limpia o reinicia el codigo
            myTextBox.Text = string.Empty;
        }

        // Guardar archivo
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, myTextBox.Text);
            }
        }

        // Guardar archivo como...
        private void SaveAsFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, myTextBox.Text);
            }
        }

        // Abrir archivo
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                myTextBox.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }
        #endregion 
    }
}
