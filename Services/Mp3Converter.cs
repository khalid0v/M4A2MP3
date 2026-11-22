using System.Diagnostics;
using System.IO;

namespace M4A2MP3.Services;

public static class Mp3Converter
{
    /// <summary>Returns true if ffmpeg can be launched from the PATH.</summary>
    public static async Task<bool> IsFfmpegAvailableAsync()
    {
        try
        {
            var psi = new ProcessStartInfo("ffmpeg", "-version")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process is null) return false;

            await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Converts one M4A file to MP3 using ffmpeg (libmp3lame).</summary>
    public static async Task<(bool Success, string Error)> ConvertAsync(
        string input, string output, string bitrate)
    {
        var psi = new ProcessStartInfo("ffmpeg")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = false
        };

        // ArgumentList handles quoting for paths with spaces.
        psi.ArgumentList.Add("-nostdin");
        psi.ArgumentList.Add("-y");
        psi.ArgumentList.Add("-i");
        psi.ArgumentList.Add(input);
        psi.ArgumentList.Add("-vn");                 // drop video/cover-art stream
        psi.ArgumentList.Add("-codec:a");
        psi.ArgumentList.Add("libmp3lame");
        psi.ArgumentList.Add("-b:a");
        psi.ArgumentList.Add(bitrate);
        psi.ArgumentList.Add(output);

        try
        {
            var dir = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            using var process = Process.Start(psi);
            if (process is null)
                return (false, "Could not start ffmpeg.");

            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
                return (true, string.Empty);

            var lastLine = stderr
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault()?.Trim() ?? "Unknown ffmpeg error.";

            return (false, lastLine);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
