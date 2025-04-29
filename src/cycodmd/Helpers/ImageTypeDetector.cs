using System;
using System.IO;

class ImageTypeDetector
{
    public static string GetContentType(string fileName)
    {
        var imageBytes = File.ReadAllBytes(fileName);
        return GetContentType(fileName, imageBytes);
    }

    public static string GetContentType(string fileName, byte[] imageBytes)
    {
        return GetContentTypeFromImage(imageBytes) ?? GetContentTypeFromFileName(fileName);
    }

    public static string GetContentTypeFromImage(byte[] imageBytes)
    {
        var imageType = GetImageType(imageBytes);
        return imageType switch
        {
            ImageType.Jpg => "image/jpg",
            ImageType.Png => "image/png",
            ImageType.Gif => "image/gif",
            ImageType.Bmp => "image/bmp",
            _ => null
        };
    }

    public static string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".jpg" => "image/jpg",
            ".jpeg" => "image/jpg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => null
        };
    }

    public static ImageType GetImageType(byte[] imageBytes)
    {
        if (imageBytes.Length < 4)
        {
            return ImageType.Unknown;
        }

        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
        {
            return ImageType.Jpg;
        }

        if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
        {
            return ImageType.Png;
        }

        if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46)
        {
            return ImageType.Gif;
        }

        if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
        {
            return ImageType.Bmp;
        }

        return ImageType.Unknown;
    }
}
