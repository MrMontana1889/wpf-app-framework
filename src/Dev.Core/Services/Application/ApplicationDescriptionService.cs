// ApplicationDescriptionService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Services;

/// <summary>
/// Concrete implementation of <see cref="IApplicationDescriptionService"/>.
/// Constructed with the host application's identifying values at startup.
/// </summary>
public class ApplicationDescriptionService : IApplicationDescriptionService
{
    public ApplicationDescriptionService(string applicationName)
    {
        ApplicationName = applicationName;
    }

    public string ApplicationName { get; }
}
