using Microsoft.VisualBasic;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
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
using Path = System.IO.Path;

namespace Document_Management_System_with_UI
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private int documentId;
        public UserWindow()
        {
            InitializeComponent();
        }

        private string loggedInUsername;

        public UserWindow(string username)
        {
            InitializeComponent();

            loggedInUsername = username;
            UserLabel.Content = loggedInUsername;
        }

        private bool CheckUserAccessLevel(string username, string requiredAccessLevel)
        {
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string selectQuery = "SELECT accesslevel FROM dms.user WHERE username = @Username";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string userAccessLevel = result.ToString();
                            if (userAccessLevel == requiredAccessLevel && username == loggedInUsername)
                            {
                                return true; // User has the required access level and matches the logged-in username
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while checking the user's access level: " + ex.Message);
                }
            }

            return false; // User does not have the required access level or does not match the logged-in username, or an error occurred
        }

        private void Home_Button_Click(object sender, RoutedEventArgs e)
        {
            string username = loggedInUsername;
            HomeWindow objHomeWindow = new HomeWindow(username);
            this.Visibility = Visibility.Hidden;
            objHomeWindow.Show();
        }

        private void VH_Button_Click(object sender, RoutedEventArgs e)
        {
            bool hasViewAndEditAccess = CheckUserAccessLevel(loggedInUsername, "View & Edit");

            if (!hasViewAndEditAccess)
            {
                MessageBox.Show("You lack the permission to access version history.");
                return;
            }

            string username = loggedInUsername;
            VersionWindow objVersionWindow = new VersionWindow(username);
            this.Visibility = Visibility.Hidden;
            objVersionWindow.Show();
        }

        private void MU_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Logout_Button_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow objMainWindow = new LoginWindow();
            Window.GetWindow(this).Close();
            objMainWindow.Show();
        }

        private void Window3_Loaded(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData(string searchText = null)
        {
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string selectQuery = "SELECT username, create_time, accesslevel FROM dms.user";

            // If there's a search text, modify the SELECT query to include the WHERE clause
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                selectQuery += " WHERE username LIKE @SearchText";
            }

            selectQuery += " ORDER BY create_time DESC";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                    {
                        // If there's a search text, add the parameter to the command
                        if (!string.IsNullOrWhiteSpace(searchText))
                        {
                            command.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                        }

                        // Create a DataTable to hold the data from the database
                        DataTable dataTable = new DataTable();

                        // Use a MySqlDataAdapter to fill the DataTable with the data
                        MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command);
                        dataAdapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                        {
                            dgvdocument.ItemsSource = dataTable.DefaultView;
                        }
                        else
                        {
                            dgvdocument.ItemsSource = null;
                            MessageBox.Show("No documents found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void SearchChange(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            LoadData(searchText);
        }

        private void ReadOnlyAccess_Click(object sender, RoutedEventArgs e)
        {
            string selectedUsername = GetSelectedUsername();

            UpdateUserAccessLevel(selectedUsername, "Read Only");

            LoadData();
        }

        private void ViewEditAccess_Click(object sender, RoutedEventArgs e)
        {
            string selectedUsername = GetSelectedUsername();

            UpdateUserAccessLevel(selectedUsername, "View & Edit");

            LoadData();
        }

        private void UpdateUserAccessLevel(string username, string accessLevel)
        {
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";

            string updateQuery = "UPDATE dms.user SET accesslevel = @AccessLevel WHERE username = @Username";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccessLevel", accessLevel);
                        command.Parameters.AddWithValue("@Username", username);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"Access level of user '{username}' has been updated to '{accessLevel}'.");
                        }
                        else
                        {
                            MessageBox.Show($"Failed to update access level of user '{username}'.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while updating access level: " + ex.Message);
                }
            }
        }

        private string GetSelectedUsername()
        {
            DataRowView selectedRow = dgvdocument.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                return selectedRow["username"].ToString();
            }

            return null;
        }

    }
}
