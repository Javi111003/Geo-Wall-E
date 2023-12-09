using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Interpreter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var splashWindow = new SplashWindow();
            splashWindow.Closed += (s, eventArgs) =>
            {
                // Abre la ventana principal cuando se cierre la ventana de splash
                var mainWindow = new MainWindow();
                mainWindow.Show();
            };
            splashWindow.Show();
            
        }
    }
}
