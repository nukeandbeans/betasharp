using Microsoft.Extensions.Logging;

namespace BetaSharp.Util;

public class FileHelper
{
    private static readonly ILogger<FileHelper> s_logger = Log.Instance.For<FileHelper>();

    public static void CreateText(string path, string contents = "")
    {
        try
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(contents);
                sw.Flush();

                if (s_logger.IsEnabled(LogLevel.Information))
                {
                    s_logger.LogInformation("Created a new text file at {Path}", path);
                }
            }
        }
        catch (Exception exception)
        {
            s_logger.LogError("Failed to create file {Path}, {Exception}", path, exception.ToString());
        }
    }

    public static void CreateText(string path, HashSet<string> lines)
    {
        try
        {
            using (StreamWriter fs = File.CreateText(path))
            {
                foreach (string entry in lines)
                {
                    fs.WriteLine(entry);
                }

                if (s_logger.IsEnabled(LogLevel.Information))
                {
                    s_logger.LogInformation("Created a new text file at {Path}", path);
                }

                fs.Flush();
            }
        }

        catch (Exception exception)
        {
            s_logger.LogError("Failed to create file {Path}, {Exception}", path, exception.ToString());
        }
    }

    public static string ReadText(string path)
    {
        try
        {
            using (StreamReader sr = File.OpenText(path))
            {
                string s = sr.ReadToEnd();

                if (s_logger.IsEnabled(LogLevel.Information))
                {
                    s_logger.LogInformation("Read text from {Path}", path);
                }

                return s;
            }
        }
        catch (Exception exception)
        {
            s_logger.LogError("Failed to read file {Path}, {Exception}, returning an empty one", path, exception.ToString());

            return "";
        }
    }

    public static string ReadText(FileInfo info) => ReadText(info.FullName);
    public static void ReadText(Action readerFunction) => readerFunction();
    public static void CreateText(FileInfo info, string contents = "") => CreateText(info.FullName, contents);
    public static void CreateText(Action writerFunction) => writerFunction();
    public static void CreateText(FileInfo info, HashSet<string> lines) => CreateText(info.FullName, lines);
}
