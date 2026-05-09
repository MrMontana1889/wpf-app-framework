// VersionCheckService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Reflection;

namespace Dev.Core.Services;

/// <summary>
/// Implementation of version checking service.
/// </summary>
public class VersionCheckService : IVersionCheckService
{
    public string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version!;
        return $"v{version.Major:D2}.{version.Minor:D2}.{version.Build:D2}.{version.Revision:D2}";
    }
}
