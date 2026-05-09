// IApplicationDescriptionService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

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
