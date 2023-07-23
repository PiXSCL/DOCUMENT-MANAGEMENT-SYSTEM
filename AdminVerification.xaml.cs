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
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class AdminVerification : Window
    {
        public AdminVerification()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Admin.Password == "secret")
            {
                RegisterWindow objRegister = new RegisterWindow();
                Window.GetWindow(this).Close();
                objRegister.Show();

                LoginWindow loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                if (loginWindow != null)
                {
                    loginWindow.Close();
                }
            }
            else
            {
                MessageBox.Show("Invalid Secret Code");
                this.Visibility = Visibility.Hidden;
            }
        }
    }
}
