using MediaBrowser.Model.Dlna;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.CastCodecBlock;

/// <summary>
/// Runs before every API action. When the request carries a DeviceProfile
/// (PlaybackInfo, OpenLiveStream) from a matching client, removes blocked video codecs
/// from its direct play and transcoding profiles. The server then can't direct play
/// or stream-copy those codecs and transcodes to something else (H.264).
/// </summary>
public class DeviceProfileFilter : IAsyncActionFilter
{
    private const string ClientClaim = "Jellyfin-Client";
    private readonly ILogger<DeviceProfileFilter> _logger;

    public DeviceProfileFilter(ILogger<DeviceProfileFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            Apply(context);
        }
        catch (Exception ex)
        {
            // Never break playback because of this plugin.
            _logger.LogError(ex, "Cast Codec Block failed, leaving device profile untouched");
        }

        await next().ConfigureAwait(false);
    }

    private void Apply(ActionExecutingContext context)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.Enabled)
        {
            return;
        }

        DeviceProfile? profile = null;
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null)
            {
                continue;
            }

            var prop = arg.GetType().GetProperty("DeviceProfile");
            if (prop?.PropertyType == typeof(DeviceProfile) && prop.GetValue(arg) is DeviceProfile p)
            {
                profile = p;
                break;
            }
        }

        if (profile is null)
        {
            return;
        }

        var client = context.HttpContext.User.FindFirst(ClientClaim)?.Value;
        var clients = Split(config.ClientNames);
        if (string.IsNullOrEmpty(client)
            || !clients.Any(c => client.Contains(c, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var blocked = Split(config.BlockedVideoCodecs);
        if (blocked.Length == 0)
        {
            return;
        }

        // Direct play: remove codec; if nothing is left, drop the profile
        // (an empty VideoCodec means "any codec" in Jellyfin).
        profile.DirectPlayProfiles = profile.DirectPlayProfiles
            .Select(dp =>
            {
                if (dp.Type != DlnaProfileType.Video || string.IsNullOrEmpty(dp.VideoCodec))
                {
                    return dp;
                }

                dp.VideoCodec = Strip(dp.VideoCodec, blocked);
                return string.IsNullOrEmpty(dp.VideoCodec) ? null : dp;
            })
            .Where(dp => dp is not null)
            .ToArray()!;

        // Transcoding: prevents stream-copying the blocked codec into HLS
        // and prevents encoding to it.
        foreach (var tp in profile.TranscodingProfiles)
        {
            if (tp.Type != DlnaProfileType.Video || string.IsNullOrEmpty(tp.VideoCodec))
            {
                continue;
            }

            var stripped = Strip(tp.VideoCodec, blocked);
            tp.VideoCodec = string.IsNullOrEmpty(stripped) ? "h264" : stripped;
        }

        _logger.LogInformation(
            "Cast Codec Block: removed {Codecs} from device profile for client {Client}",
            string.Join(',', blocked),
            client);
    }

    private static string[] Split(string? value) =>
        (value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Strip(string codecs, string[] blocked) =>
        string.Join(',', Split(codecs).Where(c => !blocked.Contains(c, StringComparer.OrdinalIgnoreCase)));
}
