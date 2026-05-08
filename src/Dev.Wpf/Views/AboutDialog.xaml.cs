// AboutDialog.xaml.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace Dev.Wpf.Views;

[ExcludeFromCodeCoverage]
public partial class AboutDialog : BaseDialog
{
    public AboutDialog(AboutDialogViewModel viewModel, IWindowPersistenceService windowPersistence)
        : base(windowPersistence)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
