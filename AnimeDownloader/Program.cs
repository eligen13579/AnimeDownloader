
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
var input = AnsiConsole.Ask<string>("Enter [green]video URL[/] add % where episode number is:");
var from = AnsiConsole.Ask<int>("Enter [green]from[/] episode number:");
var to = AnsiConsole.Ask<int>("Enter [green]to[/] episode number:");
for (int i = from; i <= to; i++)
{
    var url = input.Replace("%", i.ToString());
    urls.Add(new Uri(url));
}
var basePath = AnsiConsole.Ask<string>("Enter the [green]base path[/] for the output files:");
var name = AnsiConsole.Ask<string>("Enter the [green]name[/] for the output files:");
var linkFinder = new LinkFinder(urls);
var videoUrls = await linkFinder.ExtractAsync();

var count = 0;
foreach (var videoUrl in videoUrls)
{
    var urlManager = new UrlManager { VideoUrl = videoUrl };
    var list = await urlManager.GetSegmentUrlList();

    var outputPath = Path.Combine(basePath, $"{name}_{count++}.mp4");
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

    
        await AnsiConsole.Status()
        .Start("Remuxing video...", async ctx =>
            await m.RemuxAsync(outputPath, newOut)
        );

        File.Delete(outputPath);
    
}
