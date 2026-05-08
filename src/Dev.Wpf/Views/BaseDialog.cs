// BaseDialog.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using System.Diagnostics.CodeAnalysis;

namespace Dev.Wpf.Views;

/// <summary>
/// Base class for modal dialogs. Inherits theming from <see cref="BaseWindow"/>
/// but suppresses window-state persistence — dialogs should not remember
/// position, size, or window state between sessions.
/// </summary>
[ExcludeFromCodeCoverage]
public class BaseDialog : BaseWindow
{
    public BaseDialog(IWindowPersistenceService windowPersistence, string? windowKey = null)
        : base(windowPersistence, windowKey)
    {
    }

    protected override bool ApplySize => false;
    protected override bool ApplyPosition => false;
    protected override bool ApplyWindowState => false;
    protected override bool SaveSize => false;
    protected override bool SavePosition => false;
    protected override bool SaveWindowState => false;
}
