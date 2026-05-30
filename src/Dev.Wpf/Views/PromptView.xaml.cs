// PromptView.xaml.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace Dev.Wpf.Views;

/// <summary>
/// Reusable prompt UI surface that can be hosted by multiple presenters.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class PromptView : UserControl
{
    public PromptView()
    {
        InitializeComponent();
    }
}
