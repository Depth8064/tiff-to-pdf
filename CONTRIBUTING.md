# Contributing

Thanks for helping improve TIFF to PDF.

## Report an issue

Use the appropriate GitHub issue form and include the affected app version, Windows version, TIFF characteristics, and exact steps to reproduce the problem. Do not attach documents containing private or sensitive information.

## Make a change

1. Create a branch from `main`.
2. Keep the change focused and follow the existing C# style.
3. Add or update tests for conversion behavior.
4. Run `./test.sh`.
5. Run `./build.sh` when the change affects the application or release output.
6. Open a pull request describing the behavior change and how it was verified.

Use Conventional Commit messages when practical, for example:

```text
fix: preserve TIFF frame resolution
```

## Project structure

- `TiffToPdf.Core/` contains conversion logic.
- `TiffToPdf.Tests/` contains xUnit tests and generated TIFF fixtures.
- `MainForm.cs` contains the Windows Forms interface.
- `.github/workflows/` contains CI and release automation.