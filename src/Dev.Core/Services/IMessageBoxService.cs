// IMessageBoxService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

/// <summary>
/// Service for displaying message boxes. Abstracted so non-UI projects can request
/// messages without taking a dependency on a UI framework.
/// </summary>
public interface IMessageBoxService
{
    void ShowInfo(string message, string title);
    void ShowWarning(string message, string title);
    void ShowError(string message, string title);
}
