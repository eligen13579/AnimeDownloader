using System.Diagnostics;

namespace AnimeDownloader;

internal class VideoRemuxer
{
    public async Task RemuxAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{inputPath}\" -c copy -bsf:a aac_adtstoasc \"{outputPath}\"",
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            string error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException($"ffmpeg fehlgeschlagen: {error}");
        }
    }
}
