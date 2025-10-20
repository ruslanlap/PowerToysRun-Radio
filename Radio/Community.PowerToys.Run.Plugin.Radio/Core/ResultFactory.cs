using Community.PowerToys.Run.Plugin.Radio.Core.Models;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Radio.Core
{
    public static class ResultFactory
    {
        public static Result Create(RadioStation station, string iconPath)
        {
            return new Result
            {
                Title = station.Name,
                SubTitle = $"{station.CountryCode} • {station.Tags} • {station.Codec} {station.Bitrate}kbps",
                IcoPath = iconPath,
                ToolTipData = new ToolTipData(
                    station.Name,
                    $"Country: {station.CountryCode}\nTags: {station.Tags}\nCodec: {station.Codec} {station.Bitrate}kbps\nURL: {station.UrlResolved}"
                ),
                Action = _ =>
                {
                    // Default: open in Windows Media Player (or default media player)
                    return Player.PlayerLauncher.PlayInMediaPlayer(station, null);
                },
                ContextData = station,
                Score = 100
            };
        }
    }
}
