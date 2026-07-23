using Avalonia;
using Avalonia.Controls.Primitives;
using KztekComponentAvalonia.Theme;

namespace KztekComponentAvalonia.Controls;

/// <summary>
/// KZTEK badge/tag control. Supports 6 variants x 2 types (StatusPill / Tag).
/// Use properties Variant and BadgeType or apply CSS classes directly.
/// </summary>
public class KzBadge : TemplatedControl
{
    public static readonly StyledProperty<string?> ContentProperty =
        AvaloniaProperty.Register<KzBadge, string?>(nameof(Content), string.Empty);

    public static readonly StyledProperty<KzBadgeVariant> VariantProperty =
        AvaloniaProperty.Register<KzBadge, KzBadgeVariant>(nameof(Variant), KzBadgeVariant.Neutral);

    public static readonly StyledProperty<KzBadgeType> BadgeTypeProperty =
        AvaloniaProperty.Register<KzBadge, KzBadgeType>(nameof(BadgeType), KzBadgeType.StatusPill);

    public string? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public KzBadgeVariant Variant
    {
        get => GetValue(VariantProperty);
        set
        {
            SetValue(VariantProperty, value);
            UpdateClasses();
        }
    }

    public KzBadgeType BadgeType
    {
        get => GetValue(BadgeTypeProperty);
        set
        {
            SetValue(BadgeTypeProperty, value);
            UpdateClasses();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateClasses();
    }

    private void UpdateClasses()
    {
        // Remove all variant classes
        Classes.Remove("kz-badge-success");
        Classes.Remove("kz-badge-warning");
        Classes.Remove("kz-badge-error");
        Classes.Remove("kz-badge-info");
        Classes.Remove("kz-badge-neutral");
        Classes.Remove("kz-badge-accent");

        // Remove type classes
        Classes.Remove("kz-badge-pill");
        Classes.Remove("kz-badge-tag");

        // Add variant
        Classes.Add(Variant switch
        {
            KzBadgeVariant.Success => "kz-badge-success",
            KzBadgeVariant.Warning => "kz-badge-warning",
            KzBadgeVariant.Error   => "kz-badge-error",
            KzBadgeVariant.Info    => "kz-badge-info",
            KzBadgeVariant.Accent  => "kz-badge-accent",
            _                      => "kz-badge-neutral"
        });

        // Add type
        Classes.Add(BadgeType == KzBadgeType.Tag ? "kz-badge-tag" : "kz-badge-pill");
    }
}
