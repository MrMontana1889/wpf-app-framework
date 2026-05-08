// PackUriIconProvider.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dev.Wpf.Controls;

/// <summary>
/// Resolves icon keys to bitmap resources embedded in an assembly via pack URIs.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PackUriIconProvider : IIconProvider
{
    private readonly string _assemblyName;
    private readonly string _resourceFolder;
    private readonly Dictionary<string, ImageSource?> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _cacheLock = new();

    public PackUriIconProvider(string assemblyName, string resourceFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceFolder);

        _assemblyName = assemblyName;
        _resourceFolder = resourceFolder.Trim('/');
    }

    public ImageSource? GetIcon(string iconKey)
    {
        if (string.IsNullOrWhiteSpace(iconKey))
            return null;

        var normalizedKey = iconKey.Trim();

        lock (_cacheLock)
        {
            if (_cache.TryGetValue(normalizedKey, out var cached))
                return cached;
        }

        var resolved = ResolveIcon(normalizedKey);

        lock (_cacheLock)
            _cache[normalizedKey] = resolved;

        return resolved;
    }

    private ImageSource? ResolveIcon(string iconKey)
    {
        try
        {
            var fileName = iconKey.EndsWith(".ico", StringComparison.OrdinalIgnoreCase)
                ? iconKey
                : $"{iconKey}.ico";

            var uri = new Uri(
                $"pack://application:,,,/{_assemblyName};component/{_resourceFolder}/{fileName}",
                UriKind.Absolute);

            if (Application.GetResourceStream(uri) is null)
                return null;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            if (bitmap.CanFreeze)
                bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
