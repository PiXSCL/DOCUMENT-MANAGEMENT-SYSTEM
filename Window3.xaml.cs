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
using System.Windows.Shapes;

namespace Document_Management_System_with_UI
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        public Window3()
        {
            InitializeComponent();
        }

        private string loggedInUsername;

        public Window3(string username)
        {
            InitializeComponent();

            loggedInUsername = username;
            UserLabel.Content = loggedInUsername;
        }

        private void Home_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VH_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MU_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Logout_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow objMainWindow = new MainWindow();
            Window.GetWindow(this).Close();
            objMainWindow.Show();
        }
    }
}
