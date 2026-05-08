// AboutDialogViewModelTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels;
using NUnit.Framework;

namespace Dev.Core.Tests.ViewModels;

[TestFixture]
public class AboutDialogViewModelTests
{
    private AboutDialogViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new AboutDialogViewModel("TestApp", "1.0.0.0");
    }

    // --- Construction ---

    [Test]
    public void Constructor_CreatesInstance()
    {
        Assert.That(_viewModel, Is.Not.Null);
    }

    [Test]
    public void ApplicationLabel_IsSetFromConstructor()
    {
        Assert.That(_viewModel.ApplicationLabel, Is.EqualTo("TestApp"));
    }

    [Test]
    public void Version_IsSetFromConstructor()
    {
        Assert.That(_viewModel.Version, Is.EqualTo("1.0.0.0"));
    }

    // --- Property defaults ---

    [Test]
    public void DialogResult_DefaultsToNull()
    {
        Assert.That(_viewModel.DialogResult, Is.Null);
    }

    // --- PropertyChanged ---

    [Test]
    public void DialogResult_RaisesPropertyChanged_WhenSet()
    {
        var raised = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AboutDialogViewModel.DialogResult))
                raised = true;
        };

        _viewModel.DialogResult = true;

        Assert.That(raised, Is.True);
    }

    // --- CloseCommand ---

    [Test]
    public void CloseCommand_IsNotNull()
    {
        Assert.That(_viewModel.CloseCommand, Is.Not.Null);
    }

    [Test]
    public void CloseCommand_CanExecute_ReturnsTrue()
    {
        Assert.That(_viewModel.CloseCommand.CanExecute(null), Is.True);
    }

    [Test]
    public void CloseCommand_Execute_SetsDialogResultToTrue()
    {
        _viewModel.CloseCommand.Execute(null);

        Assert.That(_viewModel.DialogResult, Is.True);
    }

    [Test]
    public void CloseCommand_Execute_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _viewModel.CloseCommand.Execute(null));
    }
}
