using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeDownloader;

internal class VideoDownloader
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly VideoRemuxer _videoRemuxer = new VideoRemuxer();

    public VideoDownloader()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

    }

    public async Task DownloadAsync(
        List<Uri> segmentUris,
        string outputPath,
        IProgress<(int current, int total)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.Create(outputPath);

        for (int i = 0; i < segmentUris.Count; i++)
        {
            var data = await DownloadSegmentAsync(segmentUris[i], cancellationToken);
            await fileStream.WriteAsync(data, cancellationToken);
            progress?.Report((i + 1, segmentUris.Count));
        }
    }

    private async Task<byte[]> DownloadSegmentAsync(Uri segmentUri, CancellationToken cancellationToken, int maxRetry = 3)
    {
        for (int attempt = 1; attempt <= maxRetry; attempt++)
        {
            try
            {
                var response = await _httpClient.GetByteArrayAsync(segmentUri, cancellationToken);
                return response;
            }
            catch (Exception ex) when (attempt < maxRetry)
            {
                await Task.Delay(1000, cancellationToken); // Wait before retrying
            }
        }

        throw new InvalidOperationException($"Segment konnte nach {maxRetry} Versuchen nicht geladen werden: {segmentUri}");
    }
}
