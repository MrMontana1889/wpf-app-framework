// IClipboardService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Services;

/// <summary>
/// Service for clipboard operations. Abstracted so non-UI projects can interact
/// with the clipboard without taking a dependency on a UI framework.
/// </summary>
public interface IClipboardService
{
    void SetText(string text);
    string GetText();
}
