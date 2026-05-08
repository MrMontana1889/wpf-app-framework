// WinFormsButton.cs
// Copyright (c) 2025 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Controls;

[ExcludeFromCodeCoverage]
public class WinFormsButton : Button
{
    static WinFormsButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WinFormsButton),
            new FrameworkPropertyMetadata(typeof(WinFormsButton)));
    }
}
