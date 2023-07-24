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
    public partial class HomeWindow : Window
    {
        private int documentId;
        public HomeWindow()
        {
            InitializeComponent();
        }

        private string loggedInUsername;

        public HomeWindow(string username)
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

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            bool hasViewAndEditAccess = CheckUserAccessLevel(loggedInUsername, "View & Edit");

            if (!hasViewAndEditAccess)
            {
                MessageBox.Show("You lack the permission to upload files.");
                return;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                string filePath = dlg.FileName;

                // Read the binary data of the selected file
                byte[] fileData;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileData = new byte[fileStream.Length];
                    fileStream.Read(fileData, 0, fileData.Length);
                }

                // Get the filename and extension
                string fileName = Path.GetFileName(filePath);
                string fileExtension = Path.GetExtension(filePath);

                // Insert the data into the database
                string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
                string insertQuery = "INSERT INTO dms.documents (filename, data, extension) VALUES (@FileName, @FileData, @FileExtension)";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@FileName", fileName);
                            command.Parameters.AddWithValue("@FileData", fileData);
                            command.Parameters.AddWithValue("@FileExtension", fileExtension);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                // Fetch the inserted file's id from the database
                                int fileId;
                                string getFileIdQuery = "SELECT MAX(documentid) FROM dms.documents";

                                using (MySqlCommand getFileIdCommand = new MySqlCommand(getFileIdQuery, connection))
                                {
                                    fileId = Convert.ToInt32(getFileIdCommand.ExecuteScalar());
                                }

                                // Insert the uploaded file into the versions table with description "Uploaded"
                                string author = loggedInUsername;
                                string description = "Uploaded";

                                InsertVersion(fileId, fileName, author, fileData, description);

                                MessageBox.Show("File uploaded successfully.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("File upload failed.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
            }
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


        private void Window3_Loaded(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData(string searchText = null)
        {
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string selectQuery = "SELECT filename, create_time FROM dms.documents";

            // If there's a search text, modify the SELECT query to include the WHERE clause
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                selectQuery += " WHERE filename LIKE @SearchText";
            }

            selectQuery += " ORDER BY documentid DESC";

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

                // Get the binary data from the selected row in the database
                byte[] fileData = null;
                string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
                string selectQuery = "SELECT data FROM dms.documents WHERE filename = @FileName";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@FileName", filename);
                            object result = command.ExecuteScalar();
                            if (result != null)
                            {
                                fileData = (byte[])result;
                            }
                            else
                            {
                                MessageBox.Show($"File '{filename}' not found in the database.");
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
                        MessageBox.Show("You are in read-only mode. Any changes done to this file will not be saved.");
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


        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = dgvdocument.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                string filename = selectedRow["filename"].ToString();

                // Check if the user has "View & Edit" access level
                bool hasViewAndEditAccess = CheckUserAccessLevel(loggedInUsername, "View & Edit");

                if (hasViewAndEditAccess)
                {
                    // Get the binary data from the selected row in the database
                    byte[] fileData = null;
                    string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
                    string selectQuery = "SELECT documentId, data FROM dms.documents WHERE filename = @FileName";

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();

                            using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                            {
                                command.Parameters.AddWithValue("@FileName", filename);
                                using (MySqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        documentId = reader.GetInt32("documentid");
                                        fileData = (byte[])reader["data"];
                                    }
                                    else
                                    {
                                        MessageBox.Show($"File '{filename}' not found in the database.");
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

                    // Create the temp folder if it doesn't exist
                    string tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
                    Directory.CreateDirectory(tempFolderPath);

                    // Create a temporary file path
                    string tempFilePath = Path.Combine(tempFolderPath, filename);

                    try
                    {
                        // Save the binary data to the temporary file
                        File.WriteAllBytes(tempFilePath, fileData);

                        // Use Process.Start to open the temporary file with the default associated application (read-write mode)
                        Process process = new Process();
                        process.StartInfo = new ProcessStartInfo(tempFilePath)
                        {
                            UseShellExecute = true,
                            Verb = "open",
                            WindowStyle = ProcessWindowStyle.Maximized
                        };

                        // Attach the Exited event handler to update the file data in the database after the associated application is closed
                        process.EnableRaisingEvents = true;
                        process.Exited += (s, args) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                // Read the modified file data (if any)
                                byte[] modifiedFileData = File.ReadAllBytes(tempFilePath);

                                // Get the author as the loggedInUsername
                                string author = loggedInUsername;

                                // Insert the old and new version into the version table
                                InsertVersion(documentId, filename, author, fileData, "Before Update");
                                InsertVersion(documentId, filename, author, modifiedFileData, "After Update");

                                // Update the file data in the database
                                string updateQuery = "UPDATE dms.documents SET data = @FileData WHERE filename = @FileName";

                                string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
                                using (MySqlConnection connection = new MySqlConnection(connectionString))
                                {
                                    try
                                    {
                                        connection.Open();

                                        using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@FileData", modifiedFileData);
                                            command.Parameters.AddWithValue("@FileName", filename);

                                            int rowsAffected = command.ExecuteNonQuery();
                                            if (rowsAffected > 0)
                                            {
                                                MessageBox.Show("File data updated successfully.");
                                                LoadData();
                                            }
                                            else
                                            {
                                                MessageBox.Show("File update failed.");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("An error occurred while updating the file: " + ex.Message);
                                    }
                                }

                                // Delete the temporary file after the associated application is closed
                                File.Delete(tempFilePath);
                            });
                        };

                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while opening the file: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("You lack the permission to edit files.");
                }
            }
        }


        private void InsertVersion(int fileId, string filename, string author, byte[] fileData, string description)
        {

            // Insert the data into the version table
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string insertVersionQuery = "INSERT INTO dms.versions (file_id, filename, author, data, description) VALUES (@FileId, @FileName, @Author, @FileData, @Description)";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(insertVersionQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileId", fileId);
                        command.Parameters.AddWithValue("@FileName", filename);
                        command.Parameters.AddWithValue("@Author", author);
                        command.Parameters.AddWithValue("@FileData", fileData);
                        command.Parameters.AddWithValue("@Description", description);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Version inserted successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Version insert failed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while inserting the version: " + ex.Message);
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = dgvdocument.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                // Retrieve the filename from the selected row
                string filename = selectedRow["filename"].ToString();

                // Check if the user has "View & Edit" access level
                bool hasViewAndEditAccess = CheckUserAccessLevel(loggedInUsername, "View & Edit");

                if (hasViewAndEditAccess)
                {
                    // Ask for confirmation using a MessageBox
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the file '{filename}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Get the file data from the 'documents' table before deletion
                        byte[] fileData = null;
                        int fileId = -1; // Default value to indicate fileId is not found

                        string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
                        string selectQuery = "SELECT documentid, data FROM dms.documents WHERE filename = @FileName";

                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            try
                            {
                                connection.Open();

                                using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                                {
                                    command.Parameters.AddWithValue("@FileName", filename);
                                    using (MySqlDataReader reader = command.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            fileId = reader.GetInt32("documentid");
                                            fileData = (byte[])reader["data"];
                                        }
                                        else
                                        {
                                            MessageBox.Show($"File '{filename}' not found in the database.");
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

                        // Perform the delete operation here
                        if (DeleteFileFromDatabase(filename))
                        {
                            // Insert the deletion into the versions table
                            string author = loggedInUsername;
                            string description = "Deleted";

                            InsertDeletedVersion(fileId, filename, author, fileData, description);

                            MessageBox.Show("File deleted successfully.");
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete the file.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("You lack the permission to delete files.");
                }
            }
        }

        private void InsertDeletedVersion(int fileId, string filename, string author, byte[] fileData, string description)
        {
            // Insert the data into the version table
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string insertVersionQuery = "INSERT INTO dms.versions (file_id, filename, author, data, description) VALUES (@FileId, @FileName, @Author, @FileData, @Description)";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(insertVersionQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileId", fileId);
                        command.Parameters.AddWithValue("@FileName", filename);
                        command.Parameters.AddWithValue("@Author", author);
                        command.Parameters.AddWithValue("@FileData", fileData);
                        command.Parameters.AddWithValue("@Description", description);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Deletion record inserted successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Deletion record insert failed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while inserting the deletion record: " + ex.Message);
                }
            }
        }


        private bool DeleteFileFromDatabase(string filename)
        {
            // Implement the logic to delete the file using the filename from the database
            // Here, you should perform a DELETE query to remove the file from the 'documents' table
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string deleteQuery = "DELETE FROM dms.documents WHERE filename = @FileName";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", filename);
                        int rowsAffected = command.ExecuteNonQuery();

                        // Return true if the delete operation was successful
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deleting the file: " + ex.Message);
                    return false;
                }
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            bool hasViewAndEditAccess = CheckUserAccessLevel(loggedInUsername, "View & Edit");

            if (!hasViewAndEditAccess)
            {
                MessageBox.Show("You lack the permission to create new documents.");
                return;
            }
            else
            {
                // Show a dialog to get the new document's filename from the user
                string newFileName = Interaction.InputBox("Enter the new document's filename:", "New Document", "");

                // Check if the user entered a valid filename and didn't cancel the input
                if (!string.IsNullOrWhiteSpace(newFileName))
                {
                    // Create the temp folder if it doesn't exist
                    string tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
                    Directory.CreateDirectory(tempFolderPath);

                    // Create a temporary file path with the entered filename and a default extension (e.g., .txt)
                    string tempFilePath = Path.Combine(tempFolderPath, newFileName + ".docx");

                    try
                    {
                        // Create an empty file with the entered filename in the temporary folder
                        File.Create(tempFilePath).Close();

                        // Use Process.Start to open the temporary file with the default associated application (read-write mode)
                        Process process = new Process();
                        process.StartInfo = new ProcessStartInfo(tempFilePath)
                        {
                            UseShellExecute = true,
                            Verb = "open",
                            WindowStyle = ProcessWindowStyle.Maximized
                        };

                        // Attach the Exited event handler to check if the file has been modified after the associated application is closed
                        process.EnableRaisingEvents = true;
                        process.Exited += (s, args) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                // Check if the file has been modified (if the file size is greater than 0)
                                if (new FileInfo(tempFilePath).Length > 0)
                                {
                                    // Read the modified file data
                                    byte[] modifiedFileData = File.ReadAllBytes(tempFilePath);

                                    // Save the new document data in the database
                                    string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
                                    string insertDocumentQuery = "INSERT INTO dms.documents (filename, data, extension) VALUES (@FileName, @FileData, @Extension)";
                                    string insertVersionQuery = "INSERT INTO dms.versions (file_id, filename, author, data, description) VALUES (@FileId, @FileName, @Author, @FileData, @Description)";

                                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                                    {
                                        try
                                        {
                                            connection.Open();

                                            // Insert the document data into the documents table
                                            using (MySqlCommand command = new MySqlCommand(insertDocumentQuery, connection))
                                            {
                                                command.Parameters.AddWithValue("@FileName", newFileName + ".docx");
                                                command.Parameters.AddWithValue("@FileData", modifiedFileData);
                                                command.Parameters.AddWithValue("@Extension", ".docx"); // You can change this default extension as needed

                                                int rowsAffected = command.ExecuteNonQuery();
                                                if (rowsAffected > 0)
                                                {
                                                    // Get the inserted document ID
                                                    int documentId = (int)command.LastInsertedId;

                                                    // Insert the version data into the versions table
                                                    using (MySqlCommand versionCommand = new MySqlCommand(insertVersionQuery, connection))
                                                    {
                                                        versionCommand.Parameters.AddWithValue("@FileId", documentId);
                                                        versionCommand.Parameters.AddWithValue("@FileName", newFileName + ".docx");
                                                        versionCommand.Parameters.AddWithValue("@Author", loggedInUsername);
                                                        versionCommand.Parameters.AddWithValue("@FileData", modifiedFileData);
                                                        versionCommand.Parameters.AddWithValue("@Description", "Created");

                                                        int versionRowsAffected = versionCommand.ExecuteNonQuery();
                                                        if (versionRowsAffected > 0)
                                                        {
                                                            MessageBox.Show("New document created and saved successfully.");
                                                            LoadData();
                                                        }
                                                        else
                                                        {
                                                            MessageBox.Show("Failed to save the new document.");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Failed to save the new document.");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("An error occurred while saving the new document: " + ex.Message);
                                        }
                                    }
                                }

                                // Delete the temporary file after the associated application is closed
                                File.Delete(tempFilePath);
                            });
                        };

                        // Start the associated application to allow the user to edit the new document
                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while creating the new document: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid filename. Please enter a valid filename.");
                }
            }
        }
    }
}
