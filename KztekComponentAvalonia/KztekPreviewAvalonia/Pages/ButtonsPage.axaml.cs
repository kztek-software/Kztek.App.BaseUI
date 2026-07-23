using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KztekPreviewAvalonia.Pages;

public partial class ButtonsPage : UserControl
{
    public ButtonsPage()
    {
        InitializeComponent();
    }

    private void OnBtnSaveClick(object? sender, RoutedEventArgs e)
        => ShortcutStatusText.Text = "Ctrl+S — Lưu được kích hoạt ✓";

    private void OnBtnCancelClick(object? sender, RoutedEventArgs e)
        => ShortcutStatusText.Text = "Escape — Hủy được kích hoạt ✓";

    private void OnBtnHelpClick(object? sender, RoutedEventArgs e)
        => ShortcutStatusText.Text = "F1 — Trợ giúp được kích hoạt ✓";
}
