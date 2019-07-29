#!/bin/sh

channel=preview
package_id=Xamarin.Android.Sdk

for p in curl jq; do
    if ! command -v "$p" >/dev/null; then
        printf 'error: %s not found' "$p"
        exit 1
    fi
done

release_channel_url=https://aka.ms/vs/16/release/channel
release_manifest_channelitem_id=Microsoft.VisualStudio.Manifests.VisualStudio
preview_channel_url=https://aka.ms/vs/16/pre/channel
preview_manifest_channelitem_id=Microsoft.VisualStudio.Manifests.VisualStudioPreview
case "$channel" in
    release)
        channel_url="$release_channel_url"
        manifest_channelitem_id="$release_manifest_channelitem_id"
        ;;
    preview)
        channel_url="$preview_channel_url"
        manifest_channelitem_id="$preview_manifest_channelitem_id"
        ;;
    manual)
        if [ -z "$channel_url" ]; then
            echo 'error: channel "manual" used but channel_url not set'
            exit 1
        fi
        if [ -z "$manifest_channelitem_id" ]; then
            echo 'error: channel "manual" used but manifest_channelitem_id not set'
            exit 1
        fi
        ;;
    *)
        printf 'error: unknown channel "%s"' "$channel"
        exit 1
        ;;
esac

manifest_url="$(curl -sL "$channel_url" | jq -re ".channelItems[] | select(.id==\"$manifest_channelitem_id\").payloads[0].url")"
[ $? -eq 0 ] && curl -sL "$manifest_url" | jq -re ".packages[] | select(.id==\"$package_id\").payloads[0].url"
