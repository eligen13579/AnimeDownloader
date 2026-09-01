
//Ablaufplan:
//1. video URL speichern.
//2. Basis URL speichern aus Video URL.
//3. GET VideoURL für index-...
//4. listURL erstellen auf GET Basisi URL + index-...
//5. segmentURLsList mit GET listURL erstellen
//6. Video-Datei erstellen
//7. segmentURLsList Iterieren und an Video-Datei anhängen

//Klassen:
//1-4 UrlManager
//5-7 VideoDownloader

using AnimeDownloader;
using Spectre.Console;

Console.OutputEncoding = System.Text.Encoding.UTF8;

List<Uri> urls = [];
string input = string.Empty;
do
{
    input = AnsiConsole.Ask<string>("Enter [green]video URL[/] or leave empty to continue:", "");
    if (!string.IsNullOrWhiteSpace(input))
    {
        urls.Add(new Uri(input));
    }
}
while (!string.IsNullOrWhiteSpace(input));

foreach (Uri videoUrl in urls)
{
    var urlManager = new UrlManager { VideoUrl = videoUrl };
    var list = await urlManager.GetSegmentUrlList();

    var outputPath = AnsiConsole.Ask<string>("Enter the [green]output file path (add .mp4)[/]:");
    var videoDownloader = new VideoDownloader();

    await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[green]Downloading video[/]");
            await videoDownloader.DownloadAsync(list, outputPath, new Progress<(int current, int total)>(progress =>
            {
                task.Value = (double)progress.current / progress.total * 100;
                task.MaxValue = 100;
            }));
        });

    var m = new VideoRemuxer();

    var newOut = outputPath.Substring(0, outputPath.Length - 4) + "Remux.mp4";

    if (AnsiConsole.Confirm("Do you want to remux the remux video?"))
    {
        await AnsiConsole.Status()
        .Start("Remuxing video...", async ctx =>
            m.RemuxAsync(outputPath, newOut)
        );

        File.Delete(outputPath);
    }
}
