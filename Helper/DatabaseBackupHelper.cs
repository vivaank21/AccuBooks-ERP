using System;
using System.IO;
using System.IO.Compression;
using AccuBooks_ERP.Database;

namespace AccuBooks_ERP.Helper
{
    public static class DatabaseBackupHelper
    {
        public static string CreateBackup(string targetDirectory)
        {
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            string sourceDb = AppDbContext.DbPath;
            if (!File.Exists(sourceDb))
                throw new FileNotFoundException("Source database file does not exist", sourceDb);

            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"AccuBooks_Backup_{timeStamp}.db.zip";
            string targetZipPath = Path.Combine(targetDirectory, backupFileName);

            using (var zip = ZipFile.Open(targetZipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(sourceDb, Path.GetFileName(sourceDb), CompressionLevel.Optimal);
            }

            return targetZipPath;
        }

        public static void RestoreBackup(string zipBackupPath)
        {
            if (!File.Exists(zipBackupPath))
                throw new FileNotFoundException("Backup file not found", zipBackupPath);

            string targetDb = AppDbContext.DbPath;
            string targetDirectory = Path.GetDirectoryName(targetDb)!;

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            // Extract to temp file first to verify archive integrity
            string tempDbPath = Path.Combine(targetDirectory, $"restore_temp_{Guid.NewGuid():N}.db");

            using (var zip = ZipFile.OpenRead(zipBackupPath))
            {
                var entry = zip.GetEntry(Path.GetFileName(targetDb)) ?? zip.Entries[0];
                entry.ExtractToFile(tempDbPath, overwrite: true);
            }

            // Move temp file to replace active database
            if (File.Exists(targetDb))
            {
                string oldBackup = targetDb + ".old";
                if (File.Exists(oldBackup))
                    File.Delete(oldBackup);
                File.Move(targetDb, oldBackup);
            }

            File.Move(tempDbPath, targetDb);
        }
    }
}
