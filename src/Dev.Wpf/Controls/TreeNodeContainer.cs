// TreeNodeContainer.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Dev.Wpf.Controls;

/// <summary>
/// Container element for a single <see cref="TreeNodeModel"/> node inside a
/// <see cref="TreeViewControl"/>. Wraps the node-row visuals (expand/collapse
/// glyph, optional checkbox, icon, label) and hosts a nested
/// <see cref="TreeChildItemsControl"/> for child nodes.
/// <para>
/// The container is created automatically by <see cref="TreeViewControl"/>
/// and by the inner <see cref="TreeChildItemsControl"/> used for recursive
/// child rendering; consumers do not instantiate it directly.
/// </para>
/// <para>
/// The default style is keyed by
/// <see cref="TreeViewControl.NodeContainerStyleKey"/> so the owning control
/// controls exactly which style is applied.
/// </para>
/// </summary>
[TemplatePart(Name = PartExpandCollapseGlyph, Type = typeof(ToggleButton))]
[TemplatePart(Name = PartCheckBox,            Type = typeof(CheckBox))]
[TemplatePart(Name = PartIconPresenter,       Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartLabelPresenter,      Type = typeof(TextBlock))]
[TemplatePart(Name = PartChildrenPresenter,   Type = typeof(ItemsControl))]
public class TreeNodeContainer : ContentControl
{
    private static readonly IconKeyToImageSourceConverter IconConverter = new();

    public const string PartExpandCollapseGlyph = "PART_ExpandCollapseGlyph";
    public const string PartCheckBox            = "PART_CheckBox";
    public const string PartIconPresenter       = "PART_IconPresenter";
    public const string PartLabelPresenter      = "PART_LabelPresenter";
    public const string PartChildrenPresenter   = "PART_ChildrenPresenter";

    static TreeNodeContainer()
    {
        // Point the default style lookup at the NodeContainerStyleKey entry
        // in Generic.xaml rather than an implicit TargetType-keyed style.
        // This lets TreeViewControl own and version the container appearance
        // independently of the type name.
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TreeNodeContainer),
            new FrameworkPropertyMetadata(TreeViewControl.NodeContainerStyleKey));
    }

    // -----------------------------------------------------------------------
    // Properties
    // -----------------------------------------------------------------------

    /// <summary>Reference to the root <see cref="TreeViewControl"/> that owns this container.</summary>
    internal TreeViewControl? ParentTreeView { get; set; }

    /// <summary>
    /// Convenience accessor for the bound node model via
    /// <see cref="System.Windows.FrameworkElement.DataContext"/>.
    /// </summary>
    public TreeNodeModel? Node => DataContext as TreeNodeModel;

    // -----------------------------------------------------------------------
    // Template parts
    // -----------------------------------------------------------------------

    private ToggleButton?        _expandGlyph;
    private CheckBox?            _checkBox;
    private ContentPresenter?    _iconPresenter;
    private TextBlock?           _labelPresenter;
    private TreeChildItemsControl? _childrenPresenter;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _expandGlyph       = GetTemplateChild(PartExpandCollapseGlyph) as ToggleButton;
        _checkBox          = GetTemplateChild(PartCheckBox)             as CheckBox;
        _iconPresenter     = GetTemplateChild(PartIconPresenter)        as ContentPresenter;
        _labelPresenter    = GetTemplateChild(PartLabelPresenter)       as TextBlock;
        _childrenPresenter = GetTemplateChild(PartChildrenPresenter)    as TreeChildItemsControl;

        // Propagate the root TreeViewControl reference into the children
        // presenter so containers at the next level receive it too.
        if (_childrenPresenter is not null)
            _childrenPresenter.ParentTreeView = ParentTreeView;

        ConfigureIconBinding();
    }

    private void ConfigureIconBinding()
    {
        if (_iconPresenter is null || ParentTreeView is null)
            return;

        var icon = new Image
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            SnapsToDevicePixels = true,
        };

        var binding = new MultiBinding
        {
            Converter = IconConverter,
            Mode = BindingMode.OneWay,
        };

        binding.Bindings.Add(new Binding(nameof(TreeNodeModel.IconKey)));
        binding.Bindings.Add(new Binding(nameof(TreeViewControl.IconProvider))
        {
            Source = ParentTreeView,
            Mode = BindingMode.OneWay,
        });

        BindingOperations.SetBinding(icon, Image.SourceProperty, binding);
        _iconPresenter.Content = icon;
    }
}
