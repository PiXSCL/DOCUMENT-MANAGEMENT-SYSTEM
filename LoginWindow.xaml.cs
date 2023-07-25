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
using MySql.Data.MySqlClient;

namespace Document_Management_System_with_UI
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        MySqlConnection connection = new MySqlConnection("datasource=localhost;port=3306;username=root;password=ra05182002");


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = Username.Text;
            string password = Password.Password;

            string selectQuery = "SELECT * FROM dms.user WHERE username = @Username AND password = @Password";

            MySqlCommand command = new MySqlCommand(selectQuery, connection);

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

            try
            {
                connection.Open();

                MySqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    HomeWindow objHomeWindow = new HomeWindow(username);
                    this.Visibility = Visibility.Hidden;
                    objHomeWindow.Show();
                }
                else
                {
                    MessageBox.Show("Invalid username or password. Please try again.");
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AdminVerification adminVerification = new AdminVerification();
            bool? result = adminVerification.ShowDialog();

            if (result.HasValue && result.Value)
            {
                RegisterWindow registerWindow = new RegisterWindow();
                registerWindow.Show();
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBox.Show("Authentication failed.");
            }
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

}
