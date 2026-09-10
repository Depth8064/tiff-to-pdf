using System.Drawing.Imaging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace TiffToPdf;

/// <summary>Converts multi-frame TIFF files into PDFs, preserving each frame's dimensions.</summary>
public static class TiffConverter
{
    public static void Convert(string inputPath, string outputPath, IProgress<(int Current, int Total)>? progress = null)
    {
        using var tiffImage = Image.FromFile(inputPath);
        var frameDimension = new FrameDimension(tiffImage.FrameDimensionsList[0]);
        int frameCount = tiffImage.GetFrameCount(frameDimension);

        using var document = new PdfDocument();

        for (int i = 0; i < frameCount; i++)
        {
            tiffImage.SelectActiveFrame(frameDimension, i);

            // Copy the active frame's pixels out so they survive past SelectActiveFrame calls.
            using var frameBitmap = new Bitmap(tiffImage);

            var page = document.AddPage();
            page.Width = XUnit.FromPoint(tiffImage.Width * 72d / tiffImage.HorizontalResolution);
            page.Height = XUnit.FromPoint(tiffImage.Height * 72d / tiffImage.VerticalResolution);

            // Encode as PNG (lossless) so PdfSharp embeds the image without recompressing it.
            using var pngStream = new MemoryStream();
            frameBitmap.Save(pngStream, ImageFormat.Png);
            pngStream.Position = 0;

            using var xImage = XImage.FromStream(pngStream);

            using var gfx = XGraphics.FromPdfPage(page);
            gfx.DrawImage(xImage, 0, 0, page.Width.Point, page.Height.Point);

            progress?.Report((i + 1, frameCount));
        }

        document.Save(outputPath);
    }
}
