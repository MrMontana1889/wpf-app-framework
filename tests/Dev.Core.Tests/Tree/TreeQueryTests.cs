// TreeQueryTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using NUnit.Framework;

namespace Dev.Core.Tests.Tree;

[TestFixture]
public sealed class TreeQueryTests
{
    [Test]
    public void Contains_MatchesStringProperty_CaseInsensitivelyByDefault()
    {
        var node = new TreeNodeModel("node-1", "AlphaTests");
        var query = TreeQuery.For("Label").Contains("tests");

        var matches = query.Compile()(node);

        Assert.That(matches, Is.True);
    }

    [Test]
    public void IsTrue_MatchesBooleanProperty()
    {
        var node = new TreeNodeModel("node-1", "Alpha")
        {
            IsChecked = true,
        };

        var query = TreeQuery.For("IsChecked").IsTrue();

        var matches = query.Compile()(node);

        Assert.That(matches, Is.True);
    }

    [Test]
    public void Between_MatchesNumericProperty()
    {
        var node = new TreeNodeModel("node-1", "Alpha")
        {
            SelectionIndex = 7,
        };

        var query = TreeQuery.For("SelectionIndex").Between(5, 10);

        var matches = query.Compile()(node);

        Assert.That(matches, Is.True);
    }

    [Test]
    public void And_Or_And_Not_ComposeQueriesDeterministically()
    {
        var alphaNode = new TreeNodeModel("node-1", "AlphaTests")
        {
            IsChecked = true,
        };

        var betaNode = new TreeNodeModel("node-2", "Beta")
        {
            IsChecked = false,
        };

        var composite = TreeQuery.For("Label").Contains("Alpha")
            .And(TreeQuery.For("IsChecked").IsTrue())
            .Or(TreeQuery.For("Label").Contains("Gamma"))
            .Not();

        var predicate = composite.Compile();

        Assert.Multiple(() =>
        {
            Assert.That(predicate(alphaNode), Is.False);
            Assert.That(predicate(betaNode), Is.True);
            Assert.That(predicate(alphaNode), Is.False, "query results must remain deterministic across repeated executions");
        });
    }

    [Test]
    public void EqualTo_ResolvesNestedPropertyPath_OnDerivedNodeType()
    {
        var node = new NestedTreeNodeModel("node-1", "Alpha", new NodeMetadata("Core"));
        var query = TreeQuery.For("Metadata.PartFileKey").EqualTo("Core");

        var matches = query.Compile()(node);

        Assert.That(matches, Is.True);
    }

    [Test]
    public void Compile_WithoutTerminalMatcher_ThrowsInvalidOperationException()
    {
        var query = TreeQuery.For("Label");

        Assert.That(() => query.Compile(), Throws.InvalidOperationException);
    }

    private sealed class NestedTreeNodeModel : TreeNodeModel
    {
        public NestedTreeNodeModel(string id, string label, NodeMetadata metadata)
            : base(id, label)
        {
            Metadata = metadata;
        }

        public NodeMetadata Metadata { get; }
    }

    private sealed record NodeMetadata(string PartFileKey);
}