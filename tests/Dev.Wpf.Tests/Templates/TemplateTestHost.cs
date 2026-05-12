// TemplateTestHost.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Tests.Templates;

/// <summary>
/// Creates a minimal WPF <see cref="Window"/> that hosts a
/// <see cref="FrameworkElement"/> under test, forcing a synchronous layout
/// pass so that control templates are applied and all template parts are
/// realized before the test body runs.
/// <para>
/// Dispose the host at the end of each test to close the window and release
/// the HWND. Typically used in a <c>using</c> block:
/// <code>
/// using var host = new TemplateTestHost(myControl);
/// // myControl.Template is now applied — Template.FindName works.
/// </code>
/// </para>
/// <para>
/// The window is never shown in the task bar and uses a minimal
/// <c>WindowStyle.None</c> so it does not disrupt desktop automation or CI
/// environments.
/// </para>
/// </summary>
internal sealed class TemplateTestHost : IDisposable
{
    private readonly Window _window;

    /// <summary>
    /// Initializes the host and forces a layout pass on
    /// <paramref name="content"/> so its template is fully applied.
    /// </summary>
    public TemplateTestHost(FrameworkElement content)
    {
        _window = new Window
        {
            Content          = content,
            ShowInTaskbar    = false,
            WindowStyle      = WindowStyle.None,
            SizeToContent    = SizeToContent.Manual,
            Width            = 400,
            Height           = 300,
            // Visibility.Hidden creates the HWND without making the window
            // visible on-screen, which is ideal for headless/CI execution.
            Visibility       = Visibility.Hidden,
        };
        _window.Show();

        // Belt-and-suspenders: ensure the template is applied and all
        // deferred layout operations are flushed synchronously.
        content.ApplyTemplate();
        content.UpdateLayout();
    }

    /// <inheritdoc/>
    public void Dispose() => _window.Close();

    // -----------------------------------------------------------------------
    // Helpers used by both template test fixtures
    // -----------------------------------------------------------------------

    /// <summary>
    /// Walks the visual tree depth-first from <paramref name="root"/>,
    /// returning the first element with the given
    /// <see cref="FrameworkElement.Name"/> and type
    /// <typeparamref name="T"/>, or <c>null</c> if none is found.
    /// </summary>
    public static T? FindNamedChild<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        if (root is T fe && fe.Name == name) return fe;

        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            var result = FindNamedChild<T>(child, name);
            if (result is not null) return result;
        }
        return null;
    }

    /// <summary>
    /// Walks the visual tree depth-first from <paramref name="root"/>,
    /// returning the first element of type <typeparamref name="T"/>,
    /// or <c>null</c> if none is found.
    /// </summary>
    public static T? FindChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root is T match) return match;

        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            var result = FindChild<T>(child);
            if (result is not null) return result;
        }
        return null;
    }

    /// <summary>
    /// Walks the visual tree depth-first from <paramref name="root"/>,
    /// returning all elements of type <typeparamref name="T"/> found.
    /// </summary>
    public static IEnumerable<T> FindAllChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root is T match)
            yield return match;

        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            foreach (var result in FindAllChildren<T>(child))
                yield return result;
        }
    }
}
