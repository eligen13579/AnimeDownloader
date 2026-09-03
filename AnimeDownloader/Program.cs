
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
using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    [FlagsAttribute]
    enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
    }

    static async Task Main()
    {
        // Standby verhindern (und optional auch den Bildschirm anlassen)
        SetThreadExecutionState(
            EXECUTION_STATE.ES_CONTINUOUS |
            EXECUTION_STATE.ES_SYSTEM_REQUIRED |
            EXECUTION_STATE.ES_DISPLAY_REQUIRED
        );

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
        var linkFinder = new LinkFinder(urls);
        var videoUrls = await linkFinder.ExtractAsync();
        var autoselectFirstResolution = AnsiConsole.Confirm("Do you want to [green]autoselect[/] the best resolution?");

        var count = from;
        foreach (var videoUrl in videoUrls)
        {
            var urlManager = new UrlManager(autoselectFirstResolution) { VideoUrl = videoUrl.Url };
            var list = await urlManager.GetSegmentUrlList();

            var outputPath = Path.Combine(basePath, $"{videoUrl.episodeSeason}.mp4");
            var videoDownloader = new VideoDownloader();

            await AnsiConsole.Progress()
                .Columns(
                    new SpinnerColumn()
                    {
                        Spinner = Spinner.Known.DotsCircle,
                        Style = new Style(Color.Orange1)
                    },
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn()
                    {
                        Style = new Style(Color.Orange3)
                    }
                )
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask($"[#D78700]Downloading[/]  [green]{Markup.Escape(videoUrl.Title)}[/]");
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
            File.Copy(newOut, outputPath);
            File.Delete(newOut);
            TitleDescFinder.AddDescriptionToFile(outputPath, videoUrl.Title, videoUrl.Description);
        }


        // Beim Beenden wieder auf normales Verhalten zurücksetzen
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
    }
}
