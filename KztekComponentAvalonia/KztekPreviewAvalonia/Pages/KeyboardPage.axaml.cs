using Avalonia.Controls;
using Avalonia.Interactivity;
using KztekComponentAvalonia.Controls;
using KztekComponentAvalonia.Theme;

namespace KztekPreviewAvalonia.Pages;

public partial class KeyboardPage : UserControl
{
    public KeyboardPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var keyboard = this.FindControl<KzKeyboard>("MainKeyboard");
        var input    = this.FindControl<TextBox>("MainInput");
        var status   = this.FindControl<TextBlock>("StatusLabel");
        var btnClear   = this.FindControl<KzButton>("BtnClear");
        var btnConfirm = this.FindControl<KzButton>("BtnConfirm");

        if (keyboard is not null && input is not null)
        {
            keyboard.TargetTextBox = input;

            keyboard.KeyPressed += (_, key) =>
            {
                if (key == "ENTER") DoConfirm(input, status);
            };
        }

        if (btnClear is not null && input is not null && status is not null)
            btnClear.Click += (_, _) => { input.Text = ""; status.Text = ""; };

        if (btnConfirm is not null)
            btnConfirm.Click += (_, _) => DoConfirm(input, status);
    }

    private static void DoConfirm(TextBox? input, TextBlock? status)
    {
        if (input is null || status is null) return;
        string text = (input.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            status.Foreground = KzTokens.ErrorBrush;
            status.Text       = "⚠ Vui lòng nhập nội dung trước khi xác nhận.";
        }
        else
        {
            status.Foreground = KzTokens.SuccessBrush;
            status.Text       = $"✓ Đã ghi nhận: \"{text}\"";
            input.Text        = "";
        }
    }
}
