using Microsoft.Playwright;
using TagLib;

namespace AnimeDownloader;

internal class TitleDescFinder(IPage page)
{
    private ILocator container = page.Locator("div.hosterSiteTitle").First;
    public async Task<string> GetTitle()
    {
        if (await container.Locator("span.episodeGermanTitle").CountAsync() > 0) 
        {
            return await container.Locator("span.episodeGermanTitle").InnerTextAsync();
        }
        if (await container.Locator("span.episodeEnglishTitle").CountAsync() > 0) 
        {
            return await container.Locator("span.episodeEnglishTitle").InnerTextAsync();
        }
        if (await container.Locator("small.episodeGermanTitle").CountAsync() > 0) 
        {
            return await container.Locator("small.episodeGermanTitle").InnerTextAsync();
        }
        if (await container.Locator("small.episodeEnglishTitle").CountAsync() > 0) 
        {
            return await container.Locator("small.episodeEnglishTitle").InnerTextAsync();
        }
        return "No title found";
    }

    public async Task<string> GetEpisodeSeason()
    {
        var episode = await container.GetAttributeAsync("data-episode");
        var season = await container.GetAttributeAsync("data-season");

        var episodeSeason = $"S{season}E{episode}";
        return episodeSeason;
    }

    public async Task<string> GetDescription()
    {
        var descriptionLocator = container.Locator("p.descriptionSpoiler").First;
        if (await descriptionLocator.CountAsync() > 0)
        {
            return await descriptionLocator.InnerTextAsync();
        }
        return "No description found";
    }

    public static void AddDescriptionToFile(string filePath, string title, string description)
    {
        var file = TagLib.File.Create(filePath);
        file.Tag.Title = title;
        file.Tag.Comment = description;
        file.Save();
    }
}
