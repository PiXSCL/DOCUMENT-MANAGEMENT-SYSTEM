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
using MySql.Data.MySqlClient;

namespace Document_Management_System_with_UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void Username_TextBox(object sender, TextChangedEventArgs e)
        {

        }

        MySqlConnection connection = new MySqlConnection("datasource=localhost;port=3306;username=root;password=ra05182002");


        private void Register_Button_Click(object sender, RoutedEventArgs e)
        {
            string accessLevel = string.Empty;
            string username = Username.Text.Trim();
            string password = Password.Password.Trim();
            ComboBoxItem selectedItem = Combobox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                accessLevel = selectedItem.Content.ToString();
            }


            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(accessLevel))
            {
                MessageBox.Show("Please fill in all the fields.");
                return; // Exit the method
            }

            try
            {
                string insertQuery = "INSERT INTO dms.user(username,password,accesslevel) VALUES ('" + username + "','" + password + "','" + accessLevel + "')";
                connection.Open();
                MySqlCommand command = new MySqlCommand(insertQuery, connection);
                if(command.ExecuteNonQuery()==1) 
                {
                    MessageBox.Show("Register Success");
                }
                else
                {
                    MessageBox.Show("Register Failed");
                }
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

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow objMainWindow = new MainWindow();
            Window.GetWindow(this).Close();
            objMainWindow.Show();
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

        private void Combobox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
