using Avalonia.Controls;
using Avalonia.Input;
using KztekComponentAvalonia.Controls;
using KztekPreviewAvalonia.Pages;

namespace KztekPreviewAvalonia;

public partial class MainWindow : Window
{
    private static readonly (string Label, string Icon)[] NavItems =
    {
        ("Buttons",       ""),
        ("Inputs",        ""),
        ("Badges",        ""),
        ("Cards",         ""),
        ("Numeric",       ""),
        ("Tables",        ""),
        ("Form Controls", ""),
        ("Keyboard",      ""),
        ("Menu & Tree",   ""),
    };

    private readonly Control[] _pages;

    public MainWindow()
    {
        InitializeComponent();

        _pages = new Control[]
        {
            new ButtonsPage(),
            new InputsPage(),
            new BadgesPage(),
            new CardsPage(),
            new NumericPage(),
            new SidebarPage(),
            new FormControlsPage(),
            new KeyboardPage(),
            new MenuTreePage(),
        };
    }

    protected override void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);
        SetupSidebar();
        NavigateTo(Program.StartPage);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        int idx = e.Key switch
        {
            Key.D1 or Key.NumPad1 or Key.F1 => 0,
            Key.D2 or Key.NumPad2 or Key.F2 => 1,
            Key.D3 or Key.NumPad3 or Key.F3 => 2,
            Key.D4 or Key.NumPad4 or Key.F4 => 3,
            Key.D5 or Key.NumPad5 or Key.F5 => 4,
            Key.D6 or Key.NumPad6 or Key.F6 => 5,
            Key.D7 or Key.NumPad7 or Key.F7 => 6,
            _ => -1
        };
        if (idx >= 0)
        {
            NavigateTo(idx);
            if (this.FindControl<KzSidebar>("MainSidebar") is { } sb)
                sb.SelectedIndex = idx;
        }
    }

    private void SetupSidebar()
    {
        if (this.FindControl<KzSidebar>("MainSidebar") is not { } sidebar) return;

        var items = new System.Collections.Generic.List<KzSidebarItemData>();
        foreach (var (label, icon) in NavItems)
            items.Add(new KzSidebarItemData { Label = label, Icon = icon });

        sidebar.Items = items;
        sidebar.ItemSelected += (_, data) =>
        {
            if (data is null) return;
            var idx = items.IndexOf(data);
            if (idx >= 0) NavigateTo(idx);
        };
        sidebar.SelectedIndex = Program.StartPage;
    }

    private void NavigateTo(int index)
    {
        if (this.FindControl<ContentControl>("PageHost") is { } host
            && index >= 0 && index < _pages.Length)
        {
            host.Content = _pages[index];
            if (DataContext is ViewModels.MainWindowViewModel vm)
                vm.SelectedPageIndex = index;
        }
    }
}
