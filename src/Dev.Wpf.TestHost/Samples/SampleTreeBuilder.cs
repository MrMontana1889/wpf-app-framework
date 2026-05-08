// SampleTreeBuilder.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;

namespace Dev.Wpf.TestHost.Samples;

/// <summary>
/// Builds a representative fixed-depth sample tree for exercising
/// <c>TreeViewControl</c> features in the test host.
/// <para>
/// The tree covers common scenarios: multi-level expansion, leaf nodes, a
/// non-selectable system node, and icon-key tokens that an
/// <c>IIconProvider</c> implementation would resolve.
/// </para>
/// </summary>
public static class SampleTreeBuilder
{
    public static IReadOnlyList<TreeNodeModel> Build()
    {
        // -----------------------------------------------------------------
        // Documents branch
        // -----------------------------------------------------------------
        var reports = Folder("docs.reports", "Reports");
        reports.Children.Add(File("docs.reports.q1", "Q1 Report.pdf",    "file-pdf"));
        reports.Children.Add(File("docs.reports.q2", "Q2 Report.pdf",    "file-pdf"));
        reports.Children.Add(File("docs.reports.q3", "Q3 Report.pdf",    "file-pdf"));
        reports.Children.Add(File("docs.reports.ann", "Annual Report.pdf","file-pdf"));

        var docs = Folder("docs", "Documents");
        docs.Children.Add(reports);
        docs.Children.Add(File("docs.notes",  "Notes.txt",  "file-text"));
        docs.Children.Add(File("docs.readme", "README.md",  "file-text"));

        // -----------------------------------------------------------------
        // Projects branch
        // -----------------------------------------------------------------
        var srcFolder = Folder("projects.bb.src", "src");
        srcFolder.Children.Add(Folder("projects.bb.src.devcore", "Dev.Core"));
        srcFolder.Children.Add(Folder("projects.bb.src.devwpf",  "Dev.Wpf"));
        srcFolder.Children.Add(Folder("projects.bb.src.app",     "BentleyBuildApp"));

        var testsFolder = Folder("projects.bb.tests", "tests");
        testsFolder.Children.Add(Folder("projects.bb.tests.core", "Dev.Core.Tests"));
        testsFolder.Children.Add(Folder("projects.bb.tests.wpf",  "Dev.Wpf.Tests"));

        var bb = Folder("projects.bb", "BentleyBuildApp.Next", "folder-project");
        bb.Children.Add(srcFolder);
        bb.Children.Add(testsFolder);
        bb.Children.Add(File("projects.bb.readme", "README.md", "file-text"));

        var projects = Folder("projects", "Projects");
        projects.Children.Add(bb);
        projects.Children.Add(Folder("projects.other", "OtherProject", "folder-project"));

        // -----------------------------------------------------------------
        // Settings branch (shallow — demonstrates leaf-level items)
        // -----------------------------------------------------------------
        var settings = RootFolder("settings", "Settings", "settings");
        settings.Children.Add(File("settings.theme",     "Theme",              "theme"));
        settings.Children.Add(File("settings.layout",    "Layout",             "layout"));
        settings.Children.Add(File("settings.shortcuts", "Keyboard Shortcuts", "keyboard"));

        // -----------------------------------------------------------------
        // System branch — nodes flagged as non-selectable to demonstrate
        // the IsSelectable = false contract.
        // -----------------------------------------------------------------
        var system = Folder("system", "System (read-only)", "lock");
        system.Children.Add(NonSelectable("system.cfg", "system.cfg", "file"));
        system.Children.Add(NonSelectable("system.log", "system.log", "file"));
        system.Children.Add(NonSelectable("system.ini", "system.ini", "file"));

        return [docs, projects, settings, system];
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private static TreeNodeModel Folder(string id, string label, string? iconKey = "folder") =>
        new(id, label, isExpandable: true, iconKey: iconKey);

    private static TreeNodeModel File(string id, string label, string? iconKey = "file") =>
        new(id, label, isExpandable: false, iconKey: iconKey);

    private static TreeNodeModel NonSelectable(string id, string label, string? iconKey = null) =>
        new(id, label, isExpandable: false, iconKey: iconKey) { IsSelectable = false };

    /// <summary>
    /// Creates a root-level folder that is not checkable.
    /// Used to demonstrate the IsCheckable = false contract for top-level nodes.
    /// </summary>
    private static TreeNodeModel RootFolder(string id, string label, string? iconKey = "folder") =>
        new(id, label, isExpandable: true, iconKey: iconKey) { IsCheckable = false };
}
