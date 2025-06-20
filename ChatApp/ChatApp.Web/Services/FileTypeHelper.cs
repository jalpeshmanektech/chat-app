namespace ChatApp.Web.Services;

public static class FileTypeHelper
{
    // Allowed file extensions
    private static readonly HashSet<string> AllowedExtensions = new()
    {
        // Images
        ".jpg", ".jpeg", ".png", ".bmp", ".webp",
        // GIF
        ".gif",
        // Videos
        ".mp4", ".avi", ".mov", ".wmv", ".webm", ".mkv",
        // Documents
        ".pdf",
        // Text files
        ".txt"
    };

    // Maximum file size in bytes (10MB)
    private const long MaxFileSize = 10 * 1024 * 1024;

    public static string GetFileTypeFromExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".webp" => "image",
            ".gif" => "gif",
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".webm" or ".mkv" => "video",
            ".pdf" => "pdf",
            ".txt" => "text",
            _ => "file"
        };
    }

    public static bool IsFileTypeAllowed(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }

    public static bool IsFileSizeAllowed(long fileSize)
    {
        return fileSize <= MaxFileSize;
    }

    public static string GetAllowedExtensionsString()
    {
        return string.Join(", ", AllowedExtensions);
    }
} 