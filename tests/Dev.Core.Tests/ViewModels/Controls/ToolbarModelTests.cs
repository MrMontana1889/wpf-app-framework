// ToolbarModelTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.ViewModels.Controls;
using NSubstitute;
using NUnit.Framework;

namespace Dev.Core.Tests.ViewModels.Controls;

[TestFixture]
public class ToolbarModelTests
{
    private IDialogService _dialogService = null!;
    private TestToolbarModel _model = null!;

    private sealed class TestToolbarModel : ToolbarModel
    {
        public override string Name => "Test";

        public TestToolbarModel(IDialogService dialogService)
            : base(dialogService) { }
    }

    [SetUp]
    public void SetUp()
    {
        _dialogService = Substitute.For<IDialogService>();
        _model = new TestToolbarModel(_dialogService);
    }

    // --- CustomizeCommand ---

    [Test]
    public void CustomizeCommand_IsNotNull()
    {
        Assert.That(_model.CustomizeCommand, Is.Not.Null);
    }

    [Test]
    public void CustomizeCommand_CanExecute_ReturnsTrue()
    {
        Assert.That(_model.CustomizeCommand.CanExecute(null), Is.True);
    }

    [Test]
    public void CustomizeCommand_Execute_CallsShowCustomizeToolbarDialog()
    {
        _model.CustomizeCommand.Execute(null);

        _dialogService.Received(1).ShowCustomizeToolbarDialog(_model);
    }

    [Test]
    public void CustomizeCommand_Execute_PassesModelInstanceToDialog()
    {
        ToolbarModel? captured = null;
        _dialogService.When(d => d.ShowCustomizeToolbarDialog(Arg.Any<ToolbarModel>()))
                      .Do(ci => captured = ci.Arg<ToolbarModel>());

        _model.CustomizeCommand.Execute(null);

        Assert.That(captured, Is.SameAs(_model));
    }
}
