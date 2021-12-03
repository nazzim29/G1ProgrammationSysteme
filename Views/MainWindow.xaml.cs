using EasySave_GUI.ViewModels;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
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

namespace EasySave_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ResourceDictionary EN,FR;
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            EN = new ResourceDictionary();
            FR = new ResourceDictionary();
            EN.Source = new Uri("..\\Properties\\EN.xaml", UriKind.Relative);
            FR.Source = new Uri("..\\Properties\\FR.xaml", UriKind.Relative);
            if((DataContext as ViewModel).Preferences.language == "EN")
            {
                this.Resources.MergedDictionaries.Remove(FR);
                this.Resources.MergedDictionaries.Add(EN);
            }
            else
            {
                this.Resources.MergedDictionaries.Remove(EN);
                this.Resources.MergedDictionaries.Add(FR);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
            if (dlg.ShowDialog() == true)
            {
                (DataContext as ViewModel).NewBackup.Source = dlg.SelectedPath;
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
            if (dlg.ShowDialog() == true)
            {
                (DataContext as ViewModel).NewBackup.Destination = dlg.SelectedPath;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((DataContext as ViewModel).Preferences.language == "EN")
            {
                this.Resources.MergedDictionaries.Remove(FR);
                this.Resources.MergedDictionaries.Add(EN);
            }
            else
            {
                this.Resources.MergedDictionaries.Remove(EN);
                this.Resources.MergedDictionaries.Add(FR);
            }
        }
    }
}
