// IVersionCheckService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

/// <summary>
/// Service for checking application version information.
/// </summary>
public interface IVersionCheckService
{
    /// <summary>
    /// Gets the current application version as a formatted string.
    /// </summary>
    /// <returns>The current version (e.g., "v01.02.03.04").</returns>
    string GetCurrentVersion();
}
