using Microsoft.Playwright;

namespace AnimeDownloader;


public class LinkFinder
{
    private readonly List<Uri> _videoLinks;

    public LinkFinder(List<Uri> videoLinks)
    {
        _videoLinks = videoLinks;
    }

    public async Task<List<Uri>> ExtractAsync(TimeSpan? timeoutPerVideo = null)
    {
        var gefundeneUrls = new List<Uri>();
        var timeout = timeoutPerVideo ?? TimeSpan.FromMinutes(2);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // sichtbar, damit der Nutzer klicken kann
        });

        var page = await browser.NewPageAsync();

        foreach (var link in _videoLinks)
        {
            Console.WriteLine($"Öffne: {link}");

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Handler, der auf die passende Response wartet
            void OnResponse(object? sender, IResponse response)
            {
                // Content-Type-Header prüfen
                if (response.Headers.TryGetValue("content-type", out var contentType) &&
                    contentType.Contains("vnd.apple.mpegurl", StringComparison.OrdinalIgnoreCase))
                {
                    tcs.TrySetResult(response.Url);
                }
            }

            page.Response += OnResponse;

            try
            {
                await page.GotoAsync(link.ToString());

                // Warten, bis entweder die passende Response kommt ODER Timeout erreicht wird
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

                if (completedTask == tcs.Task)
                {
                    var gefundeneUrl = await tcs.Task;
                    gefundeneUrls.Add(new Uri(gefundeneUrl));
                    Console.WriteLine($"  -> Gefunden: {gefundeneUrl}");
                }
                else
                {
                    Console.WriteLine($"  -> Timeout, kein Stream erkannt für {link}");
                }
            }
            finally
            {
                // Handler wieder entfernen, damit er nicht auf der nächsten Seite feuert
                page.Response -= OnResponse;
            }
        }

        await browser.CloseAsync();
        return gefundeneUrls;
    }
}
