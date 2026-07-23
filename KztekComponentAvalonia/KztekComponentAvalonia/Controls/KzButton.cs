using Avalonia;
using Avalonia.Controls;
using KztekComponentAvalonia.Theme;

namespace KztekComponentAvalonia.Controls;

/// <summary>
/// KZTEK branded button with variant, size, and keyboard shortcut support.
/// <para>
/// Use <c>Variant</c> and <c>KzSize</c> to control visual style, or apply CSS classes
/// directly: <c>kz-btn-primary kz-size-md</c>.
/// </para>
/// <para>
/// <b>Keyboard shortcut:</b> set <c>HotKey</c> (inherited from <see cref="Avalonia.Controls.Button"/>)
/// to register a window-level shortcut — e.g. <c>HotKey="Ctrl+S"</c> in XAML or
/// <c>button.HotKey = new KeyGesture(Key.S, KeyModifiers.Control)</c> in code.
/// The shortcut fires regardless of which control has focus.
/// </para>
/// </summary>
public class KzButton : Button
{
    public static readonly StyledProperty<KzButtonVariant> VariantProperty =
        AvaloniaProperty.Register<KzButton, KzButtonVariant>(
            nameof(Variant), KzButtonVariant.Primary);

    public static readonly StyledProperty<KzSize> KzSizeProperty =
        AvaloniaProperty.Register<KzButton, KzSize>(
            nameof(KzSize), KzSize.Md);

    public KzButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set
        {
            SetValue(VariantProperty, value);
            UpdateStyleClasses();
        }
    }

    public KzSize KzSize
    {
        get => GetValue(KzSizeProperty);
        set
        {
            SetValue(KzSizeProperty, value);
            UpdateStyleClasses();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateStyleClasses();
    }

    private void UpdateStyleClasses()
    {
        // Remove existing variant classes
        Classes.Remove("kz-btn-primary");
        Classes.Remove("kz-btn-accent");
        Classes.Remove("kz-btn-secondary");
        Classes.Remove("kz-btn-ghost");
        Classes.Remove("kz-btn-danger");

        // Remove existing size classes
        Classes.Remove("kz-size-sm");
        Classes.Remove("kz-size-md");
        Classes.Remove("kz-size-lg");

        // Add current variant
        Classes.Add(KzThemeExtensions.VariantToStyleClass(Variant));

        // Add current size
        Classes.Add(KzThemeExtensions.SizeToStyleClass(KzSize));
    }
}
