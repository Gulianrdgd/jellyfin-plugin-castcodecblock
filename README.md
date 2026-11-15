# Cast Codec Block

Jellyfin 10.11 plugin that removes selected video codecs (default `hevc,h265`) from the device profile of selected clients (default `Chromecast`). The server then can't direct play or stream-copy those codecs to that client and transcodes them to H.264 instead, at whatever bitrate you allow.

Why: some HEVC encodes turn into green smearing when cast, even on a Chromecast Ultra, while the same file plays fine in the app. Lowering the cast bitrate also forces a transcode, but caps quality for every file. This only affects the codecs and clients you list.

## How it works

An MVC action filter runs before API actions. When the request body carries a `DeviceProfile` (PlaybackInfo, OpenLiveStream) and the authenticated client name matches, it strips the blocked codecs from the direct play and transcoding profiles. Direct play profiles left with no codec are dropped, since an empty codec list means "any" in Jellyfin.

The Jellyfin cast receiver identifies itself as `Chromecast`.

## Install

1. Download the zip from Releases (or the latest workflow artifact).
2. Unzip into `<jellyfin config>/plugins/`, so you get `plugins/CastCodecBlock_1.0.0.0/`.
3. Restart Jellyfin. Configure under Dashboard → Plugins → Cast Codec Block.

When active, the log shows: `Cast Codec Block: removed hevc,h265 from device profile for client Chromecast`.

Make sure hardware encoding is enabled under Playback → Transcoding on low-power servers.

## Build

```
dotnet build Jellyfin.Plugin.CastCodecBlock -c Release
```

Tag `vX.Y.Z` to publish a release.
