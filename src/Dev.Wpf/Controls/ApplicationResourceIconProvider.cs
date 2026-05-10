// ApplicationResourceIconProvider.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Windows;
using System.Windows.Media;

namespace Dev.Wpf.Controls;

/// <summary>
/// Resolves icon keys from application resource dictionaries using a key prefix.
/// </summary>
public sealed class ApplicationResourceIconProvider : IIconProvider
{
    public ApplicationResourceIconProvider(string keyPrefix = "ToolbarIcon.")
    {
        ArgumentNullException.ThrowIfNull(keyPrefix);

        if (string.IsNullOrWhiteSpace(keyPrefix))
            throw new ArgumentException("Key prefix cannot be empty or whitespace.", nameof(keyPrefix));

        KeyPrefix = keyPrefix;
    }

    public string KeyPrefix { get; }

    public ImageSource? GetIcon(string iconKey)
    {
        if (string.IsNullOrWhiteSpace(iconKey))
            return null;

        var app = Application.Current;
        if (app is null)
            return null;

        var resourceKey = $"{KeyPrefix}{iconKey}";
        var resource = app.TryFindResource(resourceKey);

        return resource as ImageSource;
    }
}
