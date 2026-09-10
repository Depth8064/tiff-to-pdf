namespace TiffToPdf;

internal sealed class MainForm : Form
{
    private readonly Panel _dropPanel;
    private readonly Label _dropLabel;
    private readonly Button _browseButton;
    private readonly Label _statusLabel;
    private readonly ProgressBar _progressBar;

    private bool _isConverting;

    public MainForm()
    {
        Text = "Tiff to PDF";
        ClientSize = new Size(460, 260);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AllowDrop = true;

        _dropPanel = new Panel
        {
            Location = new Point(20, 20),
            Size = new Size(420, 140),
            BorderStyle = BorderStyle.FixedSingle,
            AllowDrop = true,
            BackColor = Color.WhiteSmoke,
        };
        _dropPanel.DragEnter += DropPanel_DragEnter;
        _dropPanel.DragDrop += DropPanel_DragDrop;

        _dropLabel = new Label
        {
            Text = "Drag && drop a TIFF file here",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 11f),
        };
        _dropPanel.Controls.Add(_dropLabel);

        _browseButton = new Button
        {
            Text = "Browse...",
            Location = new Point(20, 175),
            Size = new Size(120, 30),
        };
        _browseButton.Click += BrowseButton_Click;

        _progressBar = new ProgressBar
        {
            Location = new Point(20, 220),
            Size = new Size(420, 18),
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 25,
            Visible = false,
        };

        _statusLabel = new Label
        {
            Location = new Point(155, 180),
            Size = new Size(285, 24),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = string.Empty,
        };

        Controls.Add(_dropPanel);
        Controls.Add(_browseButton);
        Controls.Add(_statusLabel);
        Controls.Add(_progressBar);

        DragEnter += DropPanel_DragEnter;
        DragDrop += DropPanel_DragDrop;
    }

    private void DropPanel_DragEnter(object? sender, DragEventArgs e)
    {
        if (!_isConverting && TryGetTiffPath(e.Data, out _))
        {
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void DropPanel_DragDrop(object? sender, DragEventArgs e)
    {
        if (!_isConverting && TryGetTiffPath(e.Data, out var path))
        {
            BeginConversion(path);
        }
    }

    private static bool TryGetTiffPath(IDataObject? data, out string path)
    {
        path = string.Empty;
        if (data is null || !data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        if (data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
        {
            var ext = Path.GetExtension(files[0]).ToLowerInvariant();
            if (ext is ".tif" or ".tiff")
            {
                path = files[0];
                return true;
            }
        }

        return false;
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        if (_isConverting)
        {
            return;
        }

        using var openDialog = new OpenFileDialog
        {
            Title = "Select a TIFF file",
            Filter = "TIFF files (*.tif;*.tiff)|*.tif;*.tiff|All files (*.*)|*.*",
        };

        if (openDialog.ShowDialog(this) == DialogResult.OK)
        {
            BeginConversion(openDialog.FileName);
        }
    }

    private void BeginConversion(string inputPath)
    {
        string suggestedName = Path.GetFileNameWithoutExtension(inputPath) + ".pdf";
        using var saveDialog = new SaveFileDialog
        {
            Title = "Save PDF as",
            Filter = "PDF files (*.pdf)|*.pdf",
            FileName = suggestedName,
        };

        if (saveDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        string outputPath = saveDialog.FileName;
        _isConverting = true;
        _browseButton.Enabled = false;
        _dropLabel.Text = "Converting...";
        _progressBar.Visible = true;
        _statusLabel.Text = "Converting...";

        var progress = new Progress<(int Current, int Total)>(p =>
        {
            _statusLabel.Text = p.Total > 1
                ? $"Converted page {p.Current} of {p.Total}"
                : "Finalizing PDF...";
        });

        Task.Run(() => TiffConverter.Convert(inputPath, outputPath, progress))
            .ContinueWith(t => OnConversionComplete(t, outputPath), TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void OnConversionComplete(Task task, string outputPath)
    {
        _isConverting = false;
        _browseButton.Enabled = true;
        _dropLabel.Text = "Drag && drop a TIFF file here";
        _progressBar.Visible = false;

        if (task.IsFaulted)
        {
            var message = task.Exception?.GetBaseException().Message ?? "Unknown error.";
            _statusLabel.Text = "Conversion failed.";
            MessageBox.Show(this, $"Failed to convert file:\n{message}", "Tiff to PDF",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _statusLabel.Text = "Done!";
        ShowCompletionDialog(outputPath);
    }

    private void ShowCompletionDialog(string outputPath)
    {
        using var dialog = new CompletionDialog(outputPath);
        var result = dialog.ShowDialog(this);

        if (result == DialogResult.Yes)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outputPath) { UseShellExecute = true });
        }
        else if (result == DialogResult.No)
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
        }
    }
}

/// <summary>Small "conversion complete" dialog with Open PDF as the prominent, default action.</summary>
internal sealed class CompletionDialog : Form
{
    public CompletionDialog(string outputPath)
    {
        Text = "Tiff to PDF";
        ClientSize = new Size(360, 156);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var messageLabel = new Label
        {
            Text = $"Saved to:\n{outputPath}",
            Location = new Point(20, 15),
            Size = new Size(320, 50),
            AutoEllipsis = true,
        };

        var openPdfButton = new Button
        {
            Text = "Open PDF",
            Location = new Point(20, 80),
            Size = new Size(320, 36),
            Font = new Font(Font, FontStyle.Bold),
            DialogResult = DialogResult.Yes,
        };

        var openFolderButton = new Button
        {
            Text = "Open Folder",
            Location = new Point(20, 120),
            Size = new Size(155, 26),
            DialogResult = DialogResult.No,
        };

        var closeButton = new Button
        {
            Text = "Close",
            Location = new Point(185, 120),
            Size = new Size(155, 26),
            DialogResult = DialogResult.Cancel,
        };

        Controls.Add(messageLabel);
        Controls.Add(openPdfButton);
        Controls.Add(openFolderButton);
        Controls.Add(closeButton);

        AcceptButton = openPdfButton;
        CancelButton = closeButton;
    }
}
