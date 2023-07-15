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

namespace Document_Management_System_with_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
             
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window2 objWindow2 = new Window2();
            objWindow2.ShowDialog();
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            Passwordtextbox.Text = Password.Password;
            Password.Visibility = Visibility.Collapsed;
            Passwordtextbox.Visibility = Visibility.Visible;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            Password.Password = Passwordtextbox.Text;
            Passwordtextbox.Visibility = Visibility.Collapsed;
            Password.Visibility = Visibility.Visible;
        }
    }
}
