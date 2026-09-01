
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

var videoUrl = new Uri(
        //AnsiConsole.Ask<string>("Enter the [green]video URL[/]:")
        "https://ugc-cdn-caching-n3yghqbfxup5ihfevl.cloudwindow-route.com/engine/hls2/01/08865/pmpo7g0eb6ty_,n,.urlset/master.m3u8?t=mGpgOg0I-kLRp5oNY6ukpJdgHXDACIG6W5g71gMAdTM&s=1788261807&e=14400&f=45196985&node=FfU+Rt4APH9JdMWjBDprDVHYvabsgKPG25Nt7j3icOI=&i=91.39&sp=2500&asn=3320&q=n&rq=IjOfdPAwHScbBHKbmFmP2zDB9pn79ZNKmXtYgeku"
    );
var urlManager = new UrlManager { VideoUrl = videoUrl };
var list = await urlManager.GetSegmentUrlList();

var outputPath = AnsiConsole.Ask<string>("Enter the [green]output file path[/]:");
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
