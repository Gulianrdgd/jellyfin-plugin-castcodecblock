using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.CastCodecBlock.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Comma separated client names to match (case insensitive, substring).
    /// The Jellyfin cast receiver identifies itself as "Chromecast".
    /// </summary>
    public string ClientNames { get; set; } = "Chromecast";

    /// <summary>
    /// Comma separated video codecs to remove from those clients' profiles.
    /// </summary>
    public string BlockedVideoCodecs { get; set; } = "hevc,h265";

    public bool Enabled { get; set; } = true;
}
