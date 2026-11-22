using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace M4A2MP3.Models;

public enum ConversionStatus
{
    Pending,
    Converting,
    Done,
    Failed
}

public class AudioFile : INotifyPropertyChanged
{
    private ConversionStatus _status = ConversionStatus.Pending;
    private string _message = string.Empty;

    public AudioFile(string sourcePath)
    {
        SourcePath = sourcePath;
    }

    public string SourcePath { get; }
    public string FileName => Path.GetFileName(SourcePath);
    public string Folder => Path.GetDirectoryName(SourcePath) ?? string.Empty;

    public ConversionStatus Status
    {
        get => _status;
        set { _status = value; OnChanged(); }
    }

    public string Message
    {
        get => _message;
        set { _message = value; OnChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
