using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
using Path = System.IO.Path;

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

    private void Upload_Click(object sender, RoutedEventArgs e)
    {
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
        private void Window3_Loaded(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            string connectionString = "datasource=localhost;port=3306;username=root;password=ra05182002";
            string selectQuery = "SELECT filename, create_time FROM dms.documents";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                    {
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
                            MessageBox.Show("Currently there's no Document");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
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
            // Get the selected item from the DataGridView
            DataRowView selectedRow = dgvdocument.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                // Retrieve the filename from the selected row and implement the edit logic.
                string filename = selectedRow["filename"].ToString();
                // Implement the logic to edit the file using the filename.
                MessageBox.Show($"Editing {filename}.");
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item from the DataGridView
            DataRowView selectedRow = dgvdocument.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                // Retrieve the filename from the selected row and implement the delete logic.
                string filename = selectedRow["filename"].ToString();
                // Implement the logic to delete the file using the filename.
                MessageBox.Show($"Deleting {filename}.");
            }
        }
    }
}
