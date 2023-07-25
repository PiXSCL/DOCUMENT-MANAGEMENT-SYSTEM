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
    public partial class VersionWindow : Window
    {
        private int documentId;
        public VersionWindow()
        {
            InitializeComponent();
        }

        private string loggedInUsername;

        public VersionWindow(string username)
        {
            InitializeComponent();

            loggedInUsername = username;
            UserLabel.Content = loggedInUsername;
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

        }

        private void MU_Button_Click(object sender, RoutedEventArgs e)
        {
            AdminVerification adminVerification = new AdminVerification();
            bool? result = adminVerification.ShowDialog();

            if (result.HasValue && result.Value)
            {
                UserWindow userWindow = new UserWindow(loggedInUsername);
                userWindow.Show();
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBox.Show("Authentication failed.");
            }
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
            string selectQuery = "SELECT filename, author, description, timestamp FROM dms.versions";

            // If there's a search text, modify the SELECT query to include the WHERE clause
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                selectQuery += " WHERE filename LIKE @SearchText";
            }

            selectQuery += " ORDER BY version_id DESC";

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

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = dgvdocument.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                string filename = selectedRow["filename"].ToString();
                string author = selectedRow["author"].ToString();
                string description = selectedRow["description"].ToString();
                DateTime timestamp = (DateTime)selectedRow["timestamp"];

                // Get the binary data from the selected row in the versions table
                byte[] fileData = null;
                string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
                string selectQuery = "SELECT data FROM dms.versions WHERE filename = @FileName AND author = @Author AND description = @Description AND timestamp = @Timestamp ORDER BY timestamp DESC LIMIT 1";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@FileName", filename);
                            command.Parameters.AddWithValue("@Author", author);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@Timestamp", timestamp);

                            object result = command.ExecuteScalar();
                            if (result != null)
                            {
                                fileData = (byte[])result;
                            }
                            else
                            {
                                MessageBox.Show($"Version of file '{filename}' with author '{author}', description '{description}', and timestamp '{timestamp}' not found in the versions table.");
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while retrieving the file data: " + ex.Message);
                        return;
                    }
                }

                // Create the temp folder if it doesn't exist
                string tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
                Directory.CreateDirectory(tempFolderPath);

                // Create a temporary file path
                string tempFilePath = Path.Combine(tempFolderPath, filename);

                try
                {
                    // Save the binary data to the temporary file
                    File.WriteAllBytes(tempFilePath, fileData);

                    // Use Process.Start to open the temporary file with the default associated application (read-only mode)
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo(tempFilePath)
                    {
                        UseShellExecute = true,
                        Verb = "open",
                        WindowStyle = ProcessWindowStyle.Maximized
                    };

                    // Attach the Exited event handler to delete the temporary file after the associated application is closed
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, args) =>
                    {
                        // Delete the temporary file after the associated application is closed
                        File.Delete(tempFilePath);
                    };

                    process.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while opening the file: " + ex.Message);
                }
            }
        }

        private void Recover_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = dgvdocument.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                string filename = selectedRow["filename"].ToString();
                string author = selectedRow["author"].ToString();
                string description = selectedRow["description"].ToString();
                DateTime timestamp = (DateTime)selectedRow["timestamp"];

                // Get the binary data from the selected row in the versions table
                byte[] fileData = null;
                int fileId = -1;
                int versionId = -1;

                string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
                string selectQuery = "SELECT version_id, file_id, data FROM dms.versions WHERE filename = @FileName AND author = @Author AND description = @Description AND timestamp = @Timestamp ORDER BY timestamp DESC LIMIT 1";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@FileName", filename);
                            command.Parameters.AddWithValue("@Author", author);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@Timestamp", timestamp);

                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    documentId = reader.GetInt32("file_id");    
                                    versionId = reader.GetInt32("version_id");
                                    fileId = reader.GetInt32("file_id");
                                    fileData = (byte[])reader["data"];
                                }
                                else
                                {
                                    MessageBox.Show($"Version of file '{filename}' with author '{author}', description '{description}', and timestamp '{timestamp}' not found in the versions table.");
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while retrieving the file data: " + ex.Message);
                        return;
                    }
                }

                // Ask for confirmation using a MessageBox
                MessageBoxResult result = MessageBox.Show($"Do you want to recover the file '{filename}' with author '{author}', description '{description}', and timestamp '{timestamp}'?", "Confirm Recover", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Check if a file with the same name already exists in the documents table
                    bool fileExists = CheckFileExists(filename);

                    if (fileExists)
                    {
                        // Ask for confirmation to overwrite the existing file
                        MessageBoxResult overwriteResult = MessageBox.Show($"A file with the name '{filename}' already exists. Do you want to overwrite it?", "Confirm Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (overwriteResult == MessageBoxResult.Yes)
                        {
                            // Perform the recover (overwrite) operation here
                            if (RecoverFileInDatabase(filename, fileData, fileId))
                            {
                                // Update the version's description, timestamp, and author in the versions table
                                UpdateVersionDetails(versionId, "Recovered", DateTime.Now, loggedInUsername);

                                MessageBox.Show("File recovered and overwritten successfully.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Failed to recover the file.");
                            }
                        }
                    }
                    else
                    {
                        // Perform the recover operation here
                        if (RecoverFileInDatabase(filename, fileData))
                        {
                            // Update the version's description, timestamp, and author in the versions table
                            UpdateVersionDetails(versionId, "Recovered", DateTime.Now, loggedInUsername);

                            MessageBox.Show("File recovered successfully.");
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to recover the file.");
                        }
                    }
                }
            }
        }

        private void UpdateVersionDetails(int versionId, string description, DateTime timestamp, string author)
        {
            // Implement the logic to update the version's description, timestamp, and author in the versions table
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string updateQuery = "UPDATE dms.versions SET description = @Description, timestamp = @Timestamp, author = @Author WHERE version_id = @VersionId";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@VersionId", versionId);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@Timestamp", timestamp);
                        command.Parameters.AddWithValue("@Author", author);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected <= 0)
                        {
                            MessageBox.Show("Failed to update version details in the versions table.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while updating version details: " + ex.Message);
                }
            }

            LoadData();
        }

        private bool CheckFileExists(string filename)
        {
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string selectQuery = "SELECT COUNT(*) FROM dms.documents WHERE filename = @FileName";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", filename);
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while checking if the file exists: " + ex.Message);
                    return false;
                }
            }
        }

        private bool RecoverFileInDatabase(string filename, byte[] fileData, int fileId = -1)
        {
            // Perform the recover operation here
            // If fileId is provided (greater than -1), it means an existing version should be overwritten

            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";

            if (fileId > -1)
            {
                // Overwrite existing version
                string updateQuery = "UPDATE dms.documents SET data = @FileData WHERE documentid = @FileId";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@FileData", fileData);
                            command.Parameters.AddWithValue("@FileId", fileId);

                            int rowsAffected = command.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while recovering the file: " + ex.Message);
                        return false;
                    }
                }
            }
            else
            {
                // Insert new version
                string insertQuery = "INSERT INTO dms.documents (documentid, filename, data, extension) VALUES (@DocumentId, @FileName, @FileData, @FileExtension)";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@DocumentId", documentId);
                            command.Parameters.AddWithValue("@FileName", filename);
                            command.Parameters.AddWithValue("@FileData", fileData);
                            command.Parameters.AddWithValue("@FileExtension", Path.GetExtension(filename));

                            int rowsAffected = command.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while recovering the file: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = dgvdocument.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                // Retrieve the data from the selected row
                string filename = selectedRow["filename"].ToString();
                string author = selectedRow["author"].ToString();
                string description = selectedRow["description"].ToString();
                DateTime timestamp = (DateTime)selectedRow["timestamp"];

                // Ask for confirmation using a MessageBox
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the version of '{filename}'? After Deleting you can't recover this file again.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Perform the delete operation here
                    if (DeleteVersionFromDatabase(filename, author, description, timestamp))
                    {
                        MessageBox.Show("Version deleted successfully.");
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the version.");
                    }
                }
            }
        }

        private bool DeleteVersionFromDatabase(string filename, string author, string description, DateTime timestamp)
        {
            // Implement the logic to delete the version using the filename, author, description, and timestamp from the database
            // Here, you should perform a DELETE query to remove the version from the 'versions' table
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string deleteQuery = "DELETE FROM dms.versions WHERE filename = @FileName AND author = @Author AND description = @Description AND timestamp = @Timestamp";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", filename);
                        command.Parameters.AddWithValue("@Author", author);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@Timestamp", timestamp);

                        int rowsAffected = command.ExecuteNonQuery();

                        // Return true if the delete operation was successful
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deleting the version: " + ex.Message);
                    return false;
                }
            }
        }

    }
}
