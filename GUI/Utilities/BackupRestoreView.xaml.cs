using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AccuBooks_ERP.Helper;
using Microsoft.Win32;

namespace AccuBooks_ERP.GUI.Utilities
{
    public partial class BackupRestoreView : UserControl
    {
        public BackupRestoreView()
        {
            InitializeComponent();
            txtBackupPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "AccuBooks_Backups");
        }

        private void BtnBrowseBackup_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Backup Destination Directory"
            };

            if (dialog.ShowDialog() == true)
            {
                txtBackupPath.Text = dialog.FolderName;
            }
        }

        private void BtnCreateBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string targetDir = txtBackupPath.Text.Trim();
                if (string.IsNullOrEmpty(targetDir))
                {
                    MessageBox.Show("Please select a target backup directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string backupPath = DatabaseBackupHelper.CreateBackup(targetDir);
                MessageBox.Show($"Backup completed successfully!\nFile: {backupPath}", "Backup Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Backup generation failed.");
                MessageBox.Show($"Backup failed: {ex.Message}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBrowseRestore_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "AccuBooks Backup Archives (*.zip)|*.zip",
                Title = "Select Database Backup Archive"
            };

            if (dialog.ShowDialog() == true)
            {
                txtRestoreFilePath.Text = dialog.FileName;
            }
        }

        private void BtnRestoreDatabase_Click(object sender, RoutedEventArgs e)
        {
            string zipFile = txtRestoreFilePath.Text.Trim();
            if (string.IsNullOrEmpty(zipFile) || !File.Exists(zipFile))
            {
                MessageBox.Show("Please select a valid backup archive.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "Restoring the database will replace current records.\nAre you sure you want to proceed?",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    DatabaseBackupHelper.RestoreBackup(zipFile);
                    MessageBox.Show("Database restored successfully! Please restart the application for all changes to take effect.", "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggerService.Error(ex, "Restore failed.");
                    MessageBox.Show($"Restore failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
