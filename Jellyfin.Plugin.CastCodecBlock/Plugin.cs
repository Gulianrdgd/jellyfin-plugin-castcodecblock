using Jellyfin.Plugin.CastCodecBlock.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.CastCodecBlock;

/// <summary>
/// Strips configured video codecs from the device profile of configured clients,
/// so the server transcodes instead of sending a stream the client claims but can't decode.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "Cast Codec Block";

    public override Guid Id => Guid.Parse("3959bb87-eac3-44dd-8889-1a0e5c952d37");

    public override string Description =>
        "Removes codecs (default: HEVC) from the device profile of selected clients (default: Chromecast) so the server transcodes them to H.264.";

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
            }
        ];
    }
}
