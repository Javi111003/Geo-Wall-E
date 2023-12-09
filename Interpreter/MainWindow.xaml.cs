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
using Point = System.Windows.Point;



namespace Interpreter
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
            Utils.COLORS=new();Utils.SelectColor("black");
            Status.Text = "Estado: Listo";
            Status.Foreground = Brushes.Green;
            RunButton.IsEnabled = false;           
        }

        private void Build_Click(object sender, RoutedEventArgs e)//Botón para compilar
        {
            Utils.ClearSerials();
            var retorno = EvalHandler.Eval(myTextBox.Text);
            MessageBox.Show("the app is building");
            List<string> Errors = new();
            MessageBox.Show("Logs: " + retorno["console_log"]);
            if (!(bool)retorno["success"]) 
            {
                var error = (Exception)retorno["errors"];
                Errors.Add(error.Message);
                ErrorsList.ItemsSource = Errors;
                Status.Foreground = Brushes.Red; Status.Text = "Estado : Errores pendientes";STATUS = false;
                RunButton.IsEnabled=false;
            }
            else
            {
                Errors.Clear();
                ErrorsList.ItemsSource = null;
                RunButton.IsEnabled= true;
                STATUS = true;Status.Text = "Estado: Listo";Status.Foreground = Brushes.Green;
            }
        }

        //Abrir área de dibujo
        private void Run_Click(object sender, RoutedEventArgs e)//Botón para  dibujar
        {
            PaintingArea paintingArea = null;
            try {
                 paintingArea = new PaintingArea();
            }
            catch (Exception a) {
                MessageBox.Show(a.ToString());
            }
            Utils.LoadAllPaths(paintingArea.MyCanvas);
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
