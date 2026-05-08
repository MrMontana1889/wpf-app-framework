// IApplicationDescriptionService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Services;

/// <summary>
/// Provides identifying information about the host application.
/// </summary>
public interface IApplicationDescriptionService
{
    /// <summary>
    /// Gets the display name of the application (e.g. "BentleyBuildApp.Next").
    /// </summary>
    string ApplicationName { get; }
}
