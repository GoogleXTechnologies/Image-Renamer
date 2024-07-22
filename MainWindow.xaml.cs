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
using Microsoft.Win32;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web;


namespace ImageRenamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _uploadedFilePath;
        private const string SpecialFolder = @"C:\SpecialFolder"; // Change to your specific folder path

        public MainWindow()
        {
            InitializeComponent();
            EnsureFolderPermissions(SpecialFolder);
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == true)
            {
                _uploadedFilePath = openFileDialog.FileName;
                MessageBox.Show("Image uploaded successfully!");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_uploadedFilePath))
            {
                MessageBox.Show("Please upload an image first.");
                return;
            }

            string newFilename = FilenameTextBox.Text;
            if (string.IsNullOrEmpty(newFilename))
            {
                MessageBox.Show("Please enter a new filename.");
                return;
            }

            string encodedFilename = EncodeFilename(newFilename);
            string extension = Path.GetExtension(_uploadedFilePath);
            string newFilePath = Path.Combine(SpecialFolder, encodedFilename + extension);

            try
            {
                File.Copy(_uploadedFilePath, newFilePath);
                MessageBox.Show($"Image saved successfully as {newFilePath}");

                // Displaying decoded filename to demonstrate XSS
                string decodedFilename = DecodeFilename(encodedFilename);
                MessageBox.Show($"Decoded filename: {decodedFilename}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving image: {ex.Message}");
            }
        }

        private string EncodeFilename(string filename)
        {
            return HttpUtility.UrlEncode(filename);
        }

        private string DecodeFilename(string encodedFilename)
        {
            return HttpUtility.UrlDecode(encodedFilename);
        }

        private void EnsureFolderPermissions(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                FileSystemAccessRule accessRule = new FileSystemAccessRule(
                    everyone,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow
                );

                directorySecurity.AddAccessRule(accessRule);
                directoryInfo.SetAccessControl(directorySecurity);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access to the path '{folderPath}' is denied. Please run the application as an administrator to set folder permissions.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while setting folder permissions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}