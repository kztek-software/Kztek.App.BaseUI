using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace KztekComponentAvalonia.Controls;

/// <summary>
/// KZTEK CRUD composite control: KzToolBar (top) + DataGrid (middle) + KzNavigation (bottom).
/// Bind ItemsSource and CurrentPage/TotalPages; subscribe to toolbar events for CRUD actions.
/// </summary>
public partial class KzCrudControl : UserControl
{
    // ── Styled Properties ──────────────────────────────────────────────────────

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<KzCrudControl, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<KzCrudControl, int>(nameof(CurrentPage), 1);

    public static readonly StyledProperty<int> TotalPagesProperty =
        AvaloniaProperty.Register<KzCrudControl, int>(nameof(TotalPages), 1);

    public static readonly StyledProperty<int> TotalRowsProperty =
        AvaloniaProperty.Register<KzCrudControl, int>(nameof(TotalRows), 0);

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<KzCrudControl, object?>(nameof(SelectedItem));

    public static readonly StyledProperty<bool> ShowImportProperty =
        AvaloniaProperty.Register<KzCrudControl, bool>(nameof(ShowImport), false);

    public static readonly StyledProperty<bool> ShowExportProperty =
        AvaloniaProperty.Register<KzCrudControl, bool>(nameof(ShowExport), false);

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int TotalRows
    {
        get => GetValue(TotalRowsProperty);
        set => SetValue(TotalRowsProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public bool ShowImport
    {
        get => GetValue(ShowImportProperty);
        set => SetValue(ShowImportProperty, value);
    }

    public bool ShowExport
    {
        get => GetValue(ShowExportProperty);
        set => SetValue(ShowExportProperty, value);
    }

    // ── Inner control exposure ────────────────────────────────────────────────

    /// <summary>Direct access to the inner toolbar for visibility/label customization.</summary>
    public KzToolBar? ToolBar => _toolBar;

    /// <summary>Direct access to the inner DataGrid for column/style customization.</summary>
    public DataGrid? InnerGrid => _grid;

    /// <summary>
    /// Set columns on the inner DataGrid. Safe to call before the control is attached to
    /// the visual tree (e.g. for non-selected Avalonia TabItems): columns are stored and
    /// applied automatically in <see cref="SyncAll"/> when the tab is first shown.
    /// </summary>
    public void SetColumns(System.Collections.Generic.IEnumerable<DataGridColumn> columns)
    {
        if (_grid is not null)
        {
            _grid.Columns.Clear();
            foreach (var col in columns) _grid.Columns.Add(col);
        }
        else
        {
            _pendingColumns = new System.Collections.Generic.List<DataGridColumn>(columns);
        }
    }

    // ── Events (forwarded from KzToolBar) ─────────────────────────────────────

    public event EventHandler? AddClicked;
    public event EventHandler? EditClicked;
    public event EventHandler? DeleteClicked;
    public event EventHandler? RefreshClicked;
    public event EventHandler? ImportClicked;
    public event EventHandler? ExportClicked;
    public event EventHandler<PageChangedEventArgs>? PageChanged;
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    // ── Privates ───────────────────────────────────────────────────────────────

    private KzToolBar? _toolBar;
    private DataGrid? _grid;
    private KzNavigation? _nav;
    private TextBlock? _txtCount;
    private bool _isUpdatingSelection;
    private System.Collections.Generic.List<DataGridColumn>? _pendingColumns;

    private INotifyCollectionChanged? _subscribedCollection;
    private NotifyCollectionChangedEventHandler? _onCollectionChanged;

    private EventHandler? _onToolBarAdd;
    private EventHandler? _onToolBarEdit;
    private EventHandler? _onToolBarDelete;
    private EventHandler? _onToolBarRefresh;
    private EventHandler? _onToolBarImport;
    private EventHandler? _onToolBarExport;
    private EventHandler<PageChangedEventArgs>? _onNavPageChanged;
    private EventHandler<SelectionChangedEventArgs>? _onGridSelectionChanged;

    public KzCrudControl() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _toolBar = this.FindControl<KzToolBar>("PART_ToolBar")!;
        _grid = this.FindControl<DataGrid>("PART_Grid")!;
        _nav = this.FindControl<KzNavigation>("PART_Nav")!;
        _txtCount = this.FindControl<TextBlock>("PART_TxtCount")!;

        _onToolBarAdd = (s, e2) => AddClicked?.Invoke(s, e2);
        _onToolBarEdit = (s, e2) => EditClicked?.Invoke(s, e2);
        _onToolBarDelete = (s, e2) => DeleteClicked?.Invoke(s, e2);
        _onToolBarRefresh = (s, e2) => RefreshClicked?.Invoke(s, e2);
        _onToolBarImport = (s, e2) => ImportClicked?.Invoke(s, e2);
        _onToolBarExport = (s, e2) => ExportClicked?.Invoke(s, e2);

        _toolBar.AddClicked += _onToolBarAdd;
        _toolBar.EditClicked += _onToolBarEdit;
        _toolBar.DeleteClicked += _onToolBarDelete;
        _toolBar.RefreshClicked += _onToolBarRefresh;
        _toolBar.ImportClicked += _onToolBarImport;
        _toolBar.ExportClicked += _onToolBarExport;

        _onNavPageChanged = (_, args) =>
        {
            CurrentPage = args.NewPage;
            PageChanged?.Invoke(this, args);
        };
        _nav.PageChanged += _onNavPageChanged;

        _onGridSelectionChanged = (s, args) =>
        {
            if (_isUpdatingSelection) return;
            _isUpdatingSelection = true;
            try { SelectedItem = _grid.SelectedItem; }
            finally { _isUpdatingSelection = false; }
            SelectionChanged?.Invoke(s, args);
        };
        _grid.SelectionChanged += _onGridSelectionChanged;

        SyncAll();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_subscribedCollection is not null)
        {
            _subscribedCollection.CollectionChanged -= _onCollectionChanged;
            _subscribedCollection = null;
        }
        if (_toolBar is not null)
        {
            _toolBar.AddClicked -= _onToolBarAdd;
            _toolBar.EditClicked -= _onToolBarEdit;
            _toolBar.DeleteClicked -= _onToolBarDelete;
            _toolBar.RefreshClicked -= _onToolBarRefresh;
            _toolBar.ImportClicked -= _onToolBarImport;
            _toolBar.ExportClicked -= _onToolBarExport;
        }
        if (_nav is not null) _nav.PageChanged -= _onNavPageChanged;
        if (_grid is not null) _grid.SelectionChanged -= _onGridSelectionChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            if (_subscribedCollection is not null)
            {
                _subscribedCollection.CollectionChanged -= _onCollectionChanged;
                _subscribedCollection = null;
            }
            if (ItemsSource is INotifyCollectionChanged incc)
            {
                _subscribedCollection = incc;
                _onCollectionChanged = (_, _) => TotalRows = GetCount(ItemsSource);
                _subscribedCollection.CollectionChanged += _onCollectionChanged;
            }
            TotalRows = GetCount(ItemsSource);
            if (_grid is not null) _grid.ItemsSource = ItemsSource;
        }

        else if ((change.Property == CurrentPageProperty || change.Property == TotalPagesProperty) && _nav is not null)
        {
            _nav.CurrentPage = CurrentPage;
            _nav.TotalPages = TotalPages;
        }

        else if (change.Property == TotalRowsProperty && _txtCount is not null)
            UpdateCount();

        else if (change.Property == SelectedItemProperty && _grid is not null && !_isUpdatingSelection)
            _grid.SelectedItem = SelectedItem;

        else if ((change.Property == ShowImportProperty || change.Property == ShowExportProperty) && _toolBar is not null)
        {
            _toolBar.ShowImport = ShowImport;
            _toolBar.ShowExport = ShowExport;
        }
    }

    private void SyncAll()
    {
        if (_grid is null) return;
        if (_pendingColumns is not null)
        {
            _grid.Columns.Clear();
            foreach (var col in _pendingColumns) _grid.Columns.Add(col);
            _pendingColumns = null;
        }
        _grid.ItemsSource = ItemsSource;
        _nav!.CurrentPage = CurrentPage;
        _nav.TotalPages = TotalPages;
        TotalRows = GetCount(ItemsSource);
        UpdateCount();
        if (_toolBar is not null)
        {
            _toolBar.ShowImport = ShowImport;
            _toolBar.ShowExport = ShowExport;
        }
    }

    private void UpdateCount()
    {
        if (_txtCount is null) return;
        _txtCount.Text = $"{TotalRows} bản ghi";
    }

    private static int GetCount(IEnumerable? source)
    {
        if (source is null) return 0;
        if (source is ICollection col) return col.Count;
        return source.Cast<object>().Count();
    }
}
