# TIFF to PDF

[![CI](https://github.com/Depth8064/tiff-to-pdf/actions/workflows/ci.yml/badge.svg)](https://github.com/Depth8064/tiff-to-pdf/actions/workflows/ci.yml)
[![Release](https://github.com/Depth8064/tiff-to-pdf/actions/workflows/release.yml/badge.svg)](https://github.com/Depth8064/tiff-to-pdf/releases)

A small Windows desktop app for converting TIFF images to PDF files. Drop in a `.tif` or `.tiff`, choose where to save the PDF, and the conversion runs locally on your computer.

## Features

- Converts single-page and multi-page TIFF files
- Preserves each frame's dimensions, aspect ratio, orientation, and effective resolution
- Creates one PDF page per TIFF frame
- Shows live activity while converting and page progress for multi-page files
- Runs entirely offline
- Ships as a self-contained Windows executable with no separate .NET installation required

## Install

1. Download the latest `TiffToPdf-*-win-x64.zip` from [Releases](https://github.com/Depth8064/tiff-to-pdf/releases/latest).
2. Extract the archive.
3. Run `TiffToPdf.exe`.

Windows may show a SmartScreen warning because release binaries are not code-signed. Review the publisher information and choose **Run anyway** only if you downloaded the file from this repository.

## Use

1. Drag a TIFF file onto the window, or select **Browse**.
2. Choose the destination PDF path.
3. Wait for conversion to finish, then open the PDF or its containing folder.

The source TIFF is not modified.

## Development

Requirements:

- Windows 10 or later
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Bash, such as Git Bash

Run the tests:

```bash
./test.sh
```

Test and create a self-contained release build:

```bash
./build.sh
```

Test, build, and launch the application:

```bash
./run.sh
```

Each script accepts an optional build configuration, such as `./test.sh Debug`.

## Contributing

Bug reports and focused improvements are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.

## License

No license has been granted yet. All rights are reserved by the repository owner.