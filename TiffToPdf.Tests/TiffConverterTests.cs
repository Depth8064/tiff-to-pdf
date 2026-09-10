using PdfSharp.Pdf.IO;
using Xunit;

namespace TiffToPdf.Tests;

public class TiffConverterTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    [Fact]
    public void Convert_PortraitFrame_PreservesPhysicalDimensions()
    {
        string input = TrackTemp(TiffFixture.CreateSingleFrame(600, 900, 300, 300));
        string output = TrackTemp(TempPdfPath());

        TiffConverter.Convert(input, output);

        using var document = PdfReader.Open(output, PdfDocumentOpenMode.Import);
        Assert.Single(document.Pages);
        Assert.Equal(144, document.Pages[0].Width.Point, 1);
        Assert.Equal(216, document.Pages[0].Height.Point, 1);
    }

    [Fact]
    public void Convert_LandscapeFrame_PreservesPhysicalDimensions()
    {
        string input = TrackTemp(TiffFixture.CreateSingleFrame(900, 600, 150, 150));
        string output = TrackTemp(TempPdfPath());

        TiffConverter.Convert(input, output);

        using var document = PdfReader.Open(output, PdfDocumentOpenMode.Import);
        Assert.Single(document.Pages);
        Assert.Equal(432, document.Pages[0].Width.Point, 1);
        Assert.Equal(288, document.Pages[0].Height.Point, 1);
    }

    [Fact]
    public void Convert_MultiFrameTiff_ProducesOnePagePerFrame()
    {
        string input = TrackTemp(TiffFixture.CreateMultiFrame((600, 900), (900, 600), (400, 400)));
        string output = TrackTemp(TempPdfPath());

        TiffConverter.Convert(input, output);

        using var document = PdfReader.Open(output, PdfDocumentOpenMode.Import);
        Assert.Equal(3, document.Pages.Count);
    }

    [Fact]
    public void Convert_ReportsProgressForEachFrame()
    {
        string input = TrackTemp(TiffFixture.CreateMultiFrame((600, 900), (900, 600)));
        string output = TrackTemp(TempPdfPath());
        var reported = new List<(int Current, int Total)>();

        // Synchronous IProgress<T> so Report() invokes the callback inline, no sync-context marshaling needed.
        TiffConverter.Convert(input, output, new SynchronousProgress<(int Current, int Total)>(reported.Add));

        Assert.Equal(new[] { (1, 2), (2, 2) }, reported);
    }

    private string TrackTemp(string path)
    {
        _tempFiles.Add(path);
        return path;
    }

    private static string TempPdfPath() => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try { File.Delete(file); } catch { /* best-effort cleanup */ }
        }
    }

    private sealed class SynchronousProgress<T>(Action<T> callback) : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }
}
