using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EasySave_GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex myMutex;
        private void onload(object sender, StartupEventArgs e)
        {
            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "EasySave", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                MessageBox.Show("Already an instance is running...");
                App.Current.Shutdown();
            }
        }
    }
}
