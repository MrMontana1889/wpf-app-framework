// NewAccountData.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Wpf.TestHost.Overlays;

/// <summary>
/// Result payload returned by <see cref="NewAccountWizardOverlay"/>.
/// </summary>
public class NewAccountData
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public bool EnableFeatureA { get; set; }

    public bool EnableFeatureB { get; set; }

    public string? Notes { get; set; }
}
