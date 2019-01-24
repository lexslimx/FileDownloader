using FileDownloader.Core.Utils;
using System;
using System.IO;

namespace FileDownloader.Core.Helpers
{
    public class FileHelper
    {
        public FileType? GetFileType(string file)
        {
            FileType? fileType = null;

            if (File.Exists(file))
            {
                string mimeType = MimeMapping.MimeUtility.GetMimeMapping(file);

                if (mimeType.Contains("image"))
                    fileType = FileType.Image;
                else if (mimeType.Contains("html"))
                    fileType = FileType.Html;
                else if (mimeType.Contains("text"))
                    fileType = FileType.Text;
                else
                    fileType = FileType.Other;
            }

            return fileType;
        }

        public FileInfo SetupFile(Uri uri, string destinationFolderPath)
        {
            string urlFileName = Path.GetFileName(uri.LocalPath);

            string destinationFilePath = Path.Combine(destinationFolderPath, urlFileName);

            if (!Directory.Exists(destinationFolderPath))
                Directory.CreateDirectory(destinationFolderPath);

            int count = 1;
            string fileNameOnly = Path.GetFileNameWithoutExtension(uri.LocalPath);
            string extension = Path.GetExtension(uri.LocalPath);
            string newFullPath = destinationFolderPath;

            while (File.Exists(destinationFilePath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                destinationFilePath = Path.Combine(destinationFolderPath, tempFileName + extension);
            }

            FileInfo file = new FileInfo(destinationFilePath);

            return file;
        }
    }
}