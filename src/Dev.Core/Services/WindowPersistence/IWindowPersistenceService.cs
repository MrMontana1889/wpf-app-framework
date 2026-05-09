// IWindowPersistenceManager.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

public record WindowSettings(
    double Width,
    double Height,
    double Left,
    double Top,
    Dictionary<string, double>? Extras = null);

public interface IWindowPersistenceService
{
    WindowSettings? LoadWindowState(string key);
    void ResetWindowState(string key);
    void SaveWindowState(string key, WindowSettings settings);
}