// CustomizeToolbarViewModelTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.ViewModels;
using Dev.Core.ViewModels.Controls;
using NSubstitute;
using NUnit.Framework;
using System.Windows.Input;

namespace Dev.Core.Tests.ViewModels;

[TestFixture]
public class CustomizeToolbarViewModelTests
{
    private IDialogService _dialogService = null!;
    private IToolbarSettingsService _toolbarSettings = null!;
    private TestToolbarModel _toolbarModel = null!;
    private CustomizeToolbarViewModel _viewModel = null!;

    private sealed class TestToolbarModel : ToolbarModel
    {
        public override string Name => "Test";

        public TestToolbarModel(IDialogService dialogService) : base(dialogService)
        {
            Items.Add(new ToolbarItemModel(Substitute.For<ICommand>(), "Build",    "Build")    { IsVisible = true  });
            Items.Add(new ToolbarItemModel(Substitute.For<ICommand>(), "Pull",     "Pull")     { IsVisible = false });
            Items.Add(new ToolbarItemModel(Substitute.For<ICommand>(), "Incoming", "Incoming") { IsVisible = true  });
        }
    }

    [SetUp]
    public void SetUp()
    {
        _dialogService  = Substitute.For<IDialogService>();
        _toolbarSettings = Substitute.For<IToolbarSettingsService>();
        _toolbarModel   = new TestToolbarModel(_dialogService);
        _viewModel      = new CustomizeToolbarViewModel(_toolbarModel, _toolbarSettings);
    }

    // --- Construction ---

    [Test]
    public void Constructor_CreatesInstance()
    {
        Assert.That(_viewModel, Is.Not.Null);
    }

    [Test]
    public void ToolbarName_MatchesModelName()
    {
        Assert.That(_viewModel.ToolbarName, Is.EqualTo("Test"));
    }

    [Test]
    public void Items_CountMatchesToolbarModelItems()
    {
        Assert.That(_viewModel.Items, Has.Count.EqualTo(3));
    }

    [Test]
    public void Items_IsChecked_ReflectsInitialVisibility()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.Items[0].IsChecked, Is.True);
            Assert.That(_viewModel.Items[1].IsChecked, Is.False);
            Assert.That(_viewModel.Items[2].IsChecked, Is.True);
        });
    }

    [Test]
    public void Items_Labels_MatchToolbarModelLabels()
    {
        var labels = _viewModel.Items.Select(i => i.Label).ToList();
        Assert.That(labels, Is.EqualTo(new[] { "Build", "Pull", "Incoming" }));
    }

    // --- Apply ---

    [Test]
    public void ApplyCommand_SetsDialogResultTrue()
    {
        _viewModel.ApplyCommand.Execute(null);

        Assert.That(_viewModel.DialogResult, Is.True);
    }

    [Test]
    public void ApplyCommand_CopiesCheckedStateToToolbarModel()
    {
        _viewModel.Items[0].IsChecked = false;
        _viewModel.Items[1].IsChecked = true;

        _viewModel.ApplyCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(_toolbarModel.Items[0].IsVisible, Is.False);
            Assert.That(_toolbarModel.Items[1].IsVisible, Is.True);
            Assert.That(_toolbarModel.Items[2].IsVisible, Is.True);
        });
    }

    [Test]
    public void ApplyCommand_CallsSaveOnToolbarSettings()
    {
        _viewModel.ApplyCommand.Execute(null);

        _toolbarSettings.Received(1).Save(
            "Test",
            Arg.Is<IEnumerable<ToolbarItemModel>>(items =>
                items.Select(i => i.Name).SequenceEqual(new[] { "Build", "Pull", "Incoming" })));
    }

    [Test]
    public void ApplyCommand_DoesNotChangeOriginalVisibilityBeforeExecute()
    {
        _viewModel.Items[0].IsChecked = false;

        Assert.That(_toolbarModel.Items[0].IsVisible, Is.True);
    }

    // --- Cancel ---

    [Test]
    public void CancelCommand_SetsDialogResultFalse()
    {
        _viewModel.CancelCommand.Execute(null);

        Assert.That(_viewModel.DialogResult, Is.False);
    }

    [Test]
    public void CancelCommand_DoesNotModifyToolbarModel()
    {
        _viewModel.Items[0].IsChecked = false;
        _viewModel.Items[1].IsChecked = true;

        _viewModel.CancelCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(_toolbarModel.Items[0].IsVisible, Is.True);
            Assert.That(_toolbarModel.Items[1].IsVisible, Is.False);
        });
    }

    [Test]
    public void CancelCommand_DoesNotCallSave()
    {
        _viewModel.CancelCommand.Execute(null);

        _toolbarSettings.DidNotReceive().Save(Arg.Any<string>(), Arg.Any<IEnumerable<ToolbarItemModel>>());
    }
}
