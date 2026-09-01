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

    private readonly HttpClient _httpClient = new HttpClient();

    public UrlManager()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    }
    public async Task<List<Uri>> GetSegmentUrlList() 
    {
        if (BaseUrl == null)
        {
            SetBaseUrl();
        }
        List<Uri> segmentUrlList = [];

        var index = await GetIndex();
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

        return segmentUrlList;
    }

    private void SetBaseUrl()
    {
        var baseUrl = VideoUrl.AbsoluteUri.Substring(0, VideoUrl.AbsoluteUri.LastIndexOf('/') + 1);
        BaseUrl = new Uri(baseUrl);
    }

    private async Task<string> GetIndex()
    {
        var index = "";

        var content = await _httpClient.GetStringAsync(VideoUrl);

        if (content != null) 
        {
            var contentArray = content.Split('\n', '\r');
            foreach (var l in contentArray)
            {
                if (!l.Contains('#') && !string.IsNullOrWhiteSpace(l))
                    index = l;
            }
        }

        return index;
    }

}
