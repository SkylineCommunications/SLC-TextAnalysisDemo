using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public class FileHandler
{

    public static string FILE_FOLDER { get; set; } = @"C:\Skyline DataMiner\Webpages\Public\Files";

    public static bool CheckIfFileExists(string path)
    {
        string fileNameWithExtension = Path.GetFileName(path);

        string directoryPath = FILE_FOLDER;
        string filePath = Path.Combine(directoryPath, fileNameWithExtension);
        return File.Exists(filePath);
    }

    public static string SaveFile(string path)
    {
        string directoryPath = FILE_FOLDER;
        // Check if the directory exists; create it if it doesn't
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fileNameWithExtension = Path.GetFileName(path);
        string filePath = Path.Combine(directoryPath, fileNameWithExtension);

        File.Copy(path, filePath, true);

        Console.WriteLine($"File saved successfully at: {filePath}");

        return filePath;
    }
}
