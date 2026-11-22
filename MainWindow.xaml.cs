using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using M4A2MP3.Models;
using M4A2MP3.Services;

namespace M4A2MP3;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<AudioFile> _files = new();
    private bool _busy;

    public MainWindow()
    {
        InitializeComponent();
        FilesList.ItemsSource = _files;
        Loaded += async (_, _) => await CheckFfmpegAsync();
    }

    private async Task CheckFfmpegAsync()
    {
        if (await Mp3Converter.IsFfmpegAvailableAsync())
            return;

        MessageBox.Show(this,
            "ffmpeg was not found on your PATH.\n\n" +
            "Install it with one of:\n" +
            "  winget install ffmpeg\n" +
            "  brew install ffmpeg\n" +
            "  sudo apt install ffmpeg\n\n" +
            "Then restart this app.",
            "ffmpeg not found", MessageBoxButton.OK, MessageBoxImage.Warning);

        ConvertButton.IsEnabled = false;
        StatusText.Text = "ffmpeg is required before converting.";
    }

    // ---------- Adding / removing files ----------

    private void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;

        var dlg = new OpenFileDialog
        {
            Title = "Select M4A files",
            Multiselect = true,
            Filter = "M4A audio (*.m4a)|*.m4a|All files (*.*)|*.*"
        };

        if (dlg.ShowDialog(this) == true)
            AddPaths(dlg.FileNames);
    }

    private void AddFolder_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;

        var dlg = new OpenFolderDialog { Title = "Select a folder with M4A files" };
        if (dlg.ShowDialog(this) != true) return;

        var option = RecursiveBox.IsChecked == true
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        try
        {
            AddPaths(Directory.EnumerateFiles(dlg.FolderName, "*.m4a", option));
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(this, "Some folders could not be read due to permissions.",
                "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddPaths(IEnumerable<string> paths)
    {
        var existing = _files.Select(f => f.SourcePath)
                             .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var path in paths)
        {
            if (!File.Exists(path)) continue;
            if (!string.Equals(Path.GetExtension(path), ".m4a", StringComparison.OrdinalIgnoreCase)) continue;
            if (!existing.Add(path)) continue;

            _files.Add(new AudioFile(path));
        }

        UpdateStatus();
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;

        foreach (var item in FilesList.SelectedItems.Cast<AudioFile>().ToList())
            _files.Remove(item);

        UpdateStatus();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;

        _files.Clear();
        UpdateStatus();
    }

    private void UpdateStatus() =>
        StatusText.Text = _files.Count == 0
            ? "Add .m4a files to begin."
            : $"{_files.Count} file(s) ready.";

    // ---------- Output folder ----------

    private void BrowseOutput_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Choose output folder" };
        if (dlg.ShowDialog(this) == true)
            OutputFolderBox.Text = dlg.FolderName;
    }

    // ---------- Conversion ----------

    private async void Convert_Click(object sender, RoutedEventArgs e)
    {
        if (_files.Count == 0)
        {
            StatusText.Text = "Add some .m4a files first.";
            return;
        }

        var outputDir = OutputFolderBox.Text.Trim();
        var bitrate = (BitrateBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "320k";

        _busy = true;
        ConvertButton.IsEnabled = false;
        Progress.Maximum = _files.Count;
        Progress.Value = 0;

        int succeeded = 0, failed = 0;

        try
        {
            foreach (var file in _files.ToList())
            {
                file.Status = ConversionStatus.Converting;
                file.Message = string.Empty;
                StatusText.Text = $"Converting {file.FileName}...";

                var folder = string.IsNullOrEmpty(outputDir) ? file.Folder : outputDir;
                var output = Path.Combine(folder,
                    Path.GetFileNameWithoutExtension(file.SourcePath) + ".mp3");

                var (success, error) = await Mp3Converter.ConvertAsync(file.SourcePath, output, bitrate);

                if (success)
                {
                    file.Status = ConversionStatus.Done;
                    file.Message = output;
                    succeeded++;
                }
                else
                {
                    file.Status = ConversionStatus.Failed;
                    file.Message = error;
                    failed++;
                }

                Progress.Value++;
            }
        }
        finally
        {
            _busy = false;
            ConvertButton.IsEnabled = true;
        }

        StatusText.Text = $"Finished: {succeeded} converted, {failed} failed.";
    }
}
