using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AnimeDownloader;

internal record StreamInf()
{
    public int ProgramId { get; init; }
    public int Bandwidth { get; init; }
    public string Resolution { get; init; } = string.Empty;
    public double FrameRate { get; init; }
    public string Codecs { get; init; } = string.Empty;
    public string VideoRange { get; init; } = string.Empty;

    public static StreamInf GetStreamInf(string line)
    {
        var streamInf = new StreamInf();
        var input = line.Split(':')[1];
        var parts = Regex.Split(input, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
                  .Select(s => s.Trim())
                  .ToArray();
        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length == 2)
            {
                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim().Trim('"');
                switch (key)
                {
                    case "PROGRAM-ID":
                        streamInf = streamInf with { ProgramId = int.Parse(value) };
                        break;
                    case "BANDWIDTH":
                        streamInf = streamInf with { Bandwidth = int.Parse(value) };
                        break;
                    case "RESOLUTION":
                        streamInf = streamInf with { Resolution = value };
                        break;
                    case "FRAME-RATE":
                        streamInf = streamInf with { FrameRate = double.Parse(value) };
                        break;
                    case "CODECS":
                        streamInf = streamInf with { Codecs = value };
                        break;
                    case "VIDEO-RANGE":
                        streamInf = streamInf with { VideoRange = value };
                        break;
                }
            }
        }
        return streamInf;
    }
}
