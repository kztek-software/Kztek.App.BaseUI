using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using KztekComponentAvalonia.Theme;

namespace KztekComponentAvalonia.Controls;

/// <summary>
/// KZTEK branded TextBlock — typed typography styles (H1–Caption).
/// </summary>
public class KzLabel : TextBlock
{
    public static readonly StyledProperty<KzLabelType> LabelTypeProperty =
        AvaloniaProperty.Register<KzLabel, KzLabelType>(nameof(LabelType), KzLabelType.Body);

    public KzLabelType LabelType
    {
        get => GetValue(LabelTypeProperty);
        set
        {
            SetValue(LabelTypeProperty, value);
            ApplyStyle();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyStyle();
    }

    private void ApplyStyle()
    {
        Foreground = KzTokens.TextBrush;

        switch (LabelType)
        {
            case KzLabelType.H1:
                FontSize   = KzTokens.FontSize3xl + 6; // 36
                FontWeight = FontWeight.Bold;
                break;

            case KzLabelType.H2:
                FontSize   = KzTokens.FontSize2xl + 4; // 28
                FontWeight = FontWeight.Bold;
                break;

            case KzLabelType.H3:
                FontSize   = KzTokens.FontSizeXl;       // 20
                FontWeight = FontWeight.Bold;
                break;

            case KzLabelType.Subheading:
                FontSize   = KzTokens.FontSizeLg;       // 16
                FontWeight = FontWeight.SemiBold;
                Foreground = KzTokens.Navy700Brush;
                break;

            case KzLabelType.Small:
                FontSize   = KzTokens.FontSizeSm;       // 12
                FontWeight = FontWeight.Normal;
                break;

            case KzLabelType.Caption:
                FontSize   = KzTokens.FontSizeSm - 1;   // 11
                FontWeight = FontWeight.Normal;
                Foreground = KzTokens.TextMutedBrush;
                break;

            case KzLabelType.Muted:
                FontSize   = KzTokens.FontSizeMd;       // 14
                FontWeight = FontWeight.Normal;
                Foreground = KzTokens.TextMutedBrush;
                break;

            default: // Body
                FontSize   = KzTokens.FontSizeMd;       // 14
                FontWeight = FontWeight.Normal;
                break;
        }
    }
}
