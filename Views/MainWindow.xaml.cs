using EasySave_GUI.ViewModels;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            App.Current.Shutdown();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
           if(pwdtb.Visibility == Visibility.Collapsed)
            {
                pwdtb.Visibility = Visibility.Visible;
           pwd.Visibility = Visibility.Collapsed;

            }
            else
            {
           pwdtb.Visibility = Visibility.Collapsed;
           pwd.Visibility = Visibility.Visible;
            }
           
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void pwdtb_PasswordChanged(object sender, RoutedEventArgs e)
        {
           (DataContext as ViewModel).Preferences.Password = pwdtb.Password;
        }

        private void pwdtb_Loaded(object sender, RoutedEventArgs e)
        {
            pwdtb.Password = pwd.Text;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
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

        /*private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }*/
    }
}
