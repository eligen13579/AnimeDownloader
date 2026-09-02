using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeDownloader;

//Ablaufplan:
//1. video URL speichern.
//2. Basis URL speichern aus Video URL.
//3. GET VideoURL für index-...
//4. listURL erstellen auf GET Basisi URL + index-...

internal class UrlManager
{
    public Uri VideoUrl { get; init; }
    public Uri BaseUrl { get; private set; }

    public bool AutoselectFirstResolution { get; set; } = true;

    private readonly HttpClient _httpClient = new HttpClient();
    private bool _autoselectFirstResolution = true;

    private Func<bool, List<(string Index, StreamInf Info)>, string> _getIndex = (autoselectFirstResolution, indexInfoList) => autoselectFirstResolution switch
    {
        true => indexInfoList.First().Index,
        false => AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Please select the [green]video[/]:")
                .PageSize(10)
                .AddChoices(indexInfoList.Select(i => i.Index).ToArray())
                .UseConverter(i => indexInfoList.First(index => index.Index == i).Info.ToString())
        ),
    };

    public UrlManager(bool autoselectFirstResolution)
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        _autoselectFirstResolution = autoselectFirstResolution;
    }

    public async Task<List<Uri>> GetSegmentUrlList() 
    {
        if (BaseUrl == null)
        {
            SetBaseUrl();
        }
        List<Uri> segmentUrlList = [];
        var indexInfoList = await GetIndexInfoList();

        var index = _getIndex(AutoselectFirstResolution, indexInfoList);

        var listUrl = new Uri(BaseUrl!, index);

        var listContent = await _httpClient.GetStringAsync(listUrl);

        if (listContent != null)
        {
            var listContentArray = listContent.Split('\n', '\r');
            foreach (var l in listContentArray)
            {
                if (!l.Contains('#'))
                    segmentUrlList.Add(new Uri(BaseUrl!, l));
            }
        }

        return segmentUrlList.Take(segmentUrlList.Count - 1).ToList();
    }

    private void SetBaseUrl()
    {
        var baseUrl = VideoUrl.AbsoluteUri.Substring(0, VideoUrl.AbsoluteUri.LastIndexOf('/') + 1);
        BaseUrl = new Uri(baseUrl);
    }

    private async Task<List<(string Index, StreamInf Info)>> GetIndexInfoList()
    {
        List<string> indexList = [];
        List<StreamInf> streamInfList = [];

        var content = await _httpClient.GetStringAsync(VideoUrl);

        if (content != null) 
        {
            var contentArray = content.Split('\n', '\r');
            for ( var i = 0; i < contentArray.Length; i++) 
            {
                var c = contentArray[i];
                if (!c.Contains('#') && !string.IsNullOrWhiteSpace(c))
                {
                    indexList.Add(c);
                    streamInfList.Add(StreamInf.GetStreamInf(contentArray[i - 1]));
                }
                    
            }
        }

        List<(string Index, StreamInf Info)> indexInfoList = [];
        for (var i = 0; i < indexList.Count; i++)
        {
            indexInfoList.Add((indexList[i], streamInfList[i]));
        }
        return indexInfoList;
    }

}
