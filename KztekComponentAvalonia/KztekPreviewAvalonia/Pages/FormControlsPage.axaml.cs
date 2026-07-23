using Avalonia.Controls;
using KztekComponentAvalonia.Controls;

namespace KztekPreviewAvalonia.Pages;

public partial class FormControlsPage : UserControl
{
    public FormControlsPage()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Wire keyboard to its text input target
        if (this.FindControl<KzKeyboard>("MainKeyboard") is { } kb
            && this.FindControl<KzTextBox>("KbTarget") is { } tb)
        {
            kb.TargetTextBox = tb;
        }
    }
}
