using System.Drawing.Imaging;

namespace TiffToPdf.Tests;

/// <summary>Creates throwaway TIFF files for tests without needing checked-in binary fixtures.</summary>
internal static class TiffFixture
{
    public static string CreateSingleFrame(int width, int height, float dpiX = 96, float dpiY = 96)
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.tiff");
        using var bitmap = CreateBitmap(width, height, dpiX, dpiY);
        bitmap.Save(path, ImageFormat.Tiff);
        return path;
    }

    public static string CreateMultiFrame(params (int Width, int Height)[] frameSizes)
    {
        if (frameSizes.Length == 0)
        {
            throw new ArgumentException("At least one frame is required.", nameof(frameSizes));
        }

        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.tiff");
        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/tiff");

        using var firstBitmap = CreateBitmap(frameSizes[0].Width, frameSizes[0].Height);
        using var saveParams = new EncoderParameters(1);
        saveParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
        firstBitmap.Save(path, encoder, saveParams);

        var otherBitmaps = new List<Bitmap>();
        try
        {
            for (int i = 1; i < frameSizes.Length; i++)
            {
                var bitmap = CreateBitmap(frameSizes[i].Width, frameSizes[i].Height);
                otherBitmaps.Add(bitmap);

                using var pageParams = new EncoderParameters(1);
                pageParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                firstBitmap.SaveAdd(bitmap, pageParams);
            }

            using var flushParams = new EncoderParameters(1);
            flushParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
            firstBitmap.SaveAdd(flushParams);
        }
        finally
        {
            foreach (var bitmap in otherBitmaps)
            {
                bitmap.Dispose();
            }
        }

        return path;
    }

    private static Bitmap CreateBitmap(int width, int height, float dpiX = 96, float dpiY = 96)
    {
        var bitmap = new Bitmap(width, height);
        bitmap.SetResolution(dpiX, dpiY);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);
        g.FillRectangle(Brushes.Black, 0, 0, width / 4, height / 4);
        return bitmap;
    }
}
