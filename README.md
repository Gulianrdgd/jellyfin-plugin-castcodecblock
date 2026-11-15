# Cast Codec Block

Jellyfin 10.11 plugin that removes selected video codecs (default `hevc,h265`) from the device profile of selected clients (default `Chromecast`). The server then can't direct play or stream-copy those codecs to that client and transcodes them to H.264 instead, at whatever bitrate you allow.

Why: some HEVC encodes turn into green smearing when cast, even on a Chromecast Ultra, while the same file plays fine in the app. Lowering the cast bitrate also forces a transcode, but caps quality for every file. This only affects the codecs and clients you list.

## How it works

An MVC action filter runs before API actions. When the request body carries a `DeviceProfile` (PlaybackInfo, OpenLiveStream) and the authenticated client name matches, it strips the blocked codecs from the direct play and transcoding profiles. Direct play profiles left with no codec are dropped, since an empty codec list means "any" in Jellyfin.

The Jellyfin cast receiver identifies itself as `Chromecast`.

## Install

Add this repository in Jellyfin under Dashboard → Plugins → Repositories:

```
https://raw.githubusercontent.com/Gulianrdgd/jellyfin-plugin-castcodecblock/main/manifest.json
```

Then install Cast Codec Block from the catalog and restart Jellyfin. Configure it under Dashboard → Plugins → Cast Codec Block.

Manual install: unzip a release into `<jellyfin config>/plugins/CastCodecBlock_1.0.0.0/` and restart.

When active, the log shows: `Cast Codec Block: removed hevc,h265 from device profile for client Chromecast`.

Make sure hardware encoding is enabled under Playback → Transcoding on low-power servers.

## Build

```
dotnet build Jellyfin.Plugin.CastCodecBlock -c Release
```

Bump `AssemblyVersion` in the csproj and push to `main`. When no release exists for that version yet, the workflow tags it (`v1.0.0.1` style), publishes the release and adds it to `manifest.json`.
