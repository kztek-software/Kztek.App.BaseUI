using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace KztekComponentAvalonia.Controls;

/// <summary>Data model for a single sidebar navigation item.</summary>
public class KzSidebarItemData
{
    public string Label { get; set; } = string.Empty;
    public string? Icon  { get; set; }
    public object? Tag   { get; set; }
}

public partial class KzSidebar : UserControl
{
    // ── Styled Properties ───────────────────────────────────────────────────
    public static readonly StyledProperty<IList<KzSidebarItemData>?> ItemsProperty =
        AvaloniaProperty.Register<KzSidebar, IList<KzSidebarItemData>?>(nameof(Items));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<KzSidebar, object?>(nameof(Header));

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<KzSidebar, int>(nameof(SelectedIndex), -1);

    // ── Events ───────────────────────────────────────────────────────────────
    public event EventHandler<KzSidebarItemData?>? ItemSelected;

    // ── Properties ───────────────────────────────────────────────────────────
    public IList<KzSidebarItemData>? Items
    {
        get => GetValue(ItemsProperty);
        set { SetValue(ItemsProperty, value); RebuildItems(); }
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set
        {
            SetValue(SelectedIndexProperty, value);
            if (this.FindControl<ListBox>("NavList") is { } lb)
                lb.SelectedIndex = value;
        }
    }

    public KzSidebar()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (this.FindControl<ListBox>("NavList") is { } lb)
        {
            lb.SelectionChanged += OnSelectionChanged;
            lb.SelectedIndex = SelectedIndex;
        }

        RebuildItems();
    }

    private void RebuildItems()
    {
        if (this.FindControl<ListBox>("NavList") is not { } lb) return;
        lb.Items.Clear();
        if (Items is null) return;
        foreach (var item in Items)
            lb.Items.Add(new KzSidebarItem { ItemLabel = item.Label, ItemIcon = item.Icon, Tag = item });
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox lb)
        {
            SetValue(SelectedIndexProperty, lb.SelectedIndex);
            var data = lb.SelectedItem is KzSidebarItem item && item.Tag is KzSidebarItemData d ? d : null;
            ItemSelected?.Invoke(this, data);
        }
    }
}
