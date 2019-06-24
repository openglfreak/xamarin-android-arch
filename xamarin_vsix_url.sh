#!/bin/sh

if ! command -v curl >/dev/null; then
    echo "error: curl not found"
    exit 1
fi
if ! command -v jq >/dev/null; then
    echo "error: jq not found"
    exit 1
fi

channel_url=https://aka.ms/vs/16/pre/channel
manifest_channelitem_id=Microsoft.VisualStudio.Manifests.VisualStudioPreview
package_id=Xamarin.Android.Sdk

manifest_url="$(curl -sL "$channel_url" | jq -re ".channelItems[] | select(.id==\"$manifest_channelitem_id\").payloads[0].url")"
[ $? -eq 0 ] && curl -sL "$manifest_url" | jq -re ".packages[] | select(.id==\"$package_id\").payloads[0].url"
