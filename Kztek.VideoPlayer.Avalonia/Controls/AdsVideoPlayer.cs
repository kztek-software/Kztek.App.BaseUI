using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;

namespace Kztek.VideoPlayer.Avalonia;

/// <summary>
/// Avalonia Control hiển thị video quảng cáo phát liên tục từ <see cref="SharedAdsVideoEngine"/>.
/// Các instance AdsVideoPlayer ở nhiều màn hình khác nhau kết nối tới cùng 1 SharedAdsVideoEngine
/// để đảm bảo video phát liền mạch (seamless), không khởi tạo lại khi chuyển giao diện.
/// </summary>
public class AdsVideoPlayer : Control, IDisposable
{
    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<AdsVideoPlayer, string?>(nameof(Source));

    public static readonly StyledProperty<bool> IsLoopingProperty =
        AvaloniaProperty.Register<AdsVideoPlayer, bool>(nameof(IsLooping), defaultValue: true);

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<AdsVideoPlayer, Stretch>(nameof(Stretch), defaultValue: Stretch.Uniform);

    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public bool IsLooping
    {
        get => GetValue(IsLoopingProperty);
        set => SetValue(IsLoopingProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public bool IsPlaying => SharedAdsVideoEngine.Instance.IsPlaying;

    public event EventHandler? MediaEnded;
    public event EventHandler<string>? MediaFailed;

    public AdsVideoPlayer()
    {
        SourceProperty.Changed.AddClassHandler<AdsVideoPlayer>((player, _) =>
        {
            if (!string.IsNullOrWhiteSpace(player.Source))
            {
                SharedAdsVideoEngine.Instance.SetSource(player.Source, player.IsLooping);
            }
        });
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SharedAdsVideoEngine.Instance.FrameAvailable += OnFrameAvailable;
        if (!string.IsNullOrWhiteSpace(Source))
        {
            SharedAdsVideoEngine.Instance.SetSource(Source, IsLooping);
        }
        InvalidateVisual();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SharedAdsVideoEngine.Instance.FrameAvailable -= OnFrameAvailable;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnFrameAvailable(object? sender, WriteableBitmap e)
    {
        InvalidateVisual();
    }

    public void Play()
    {
        if (!string.IsNullOrWhiteSpace(Source))
        {
            SharedAdsVideoEngine.Instance.SetSource(Source, IsLooping);
        }
    }

    /// <summary>
    /// Unbind control khỏi shared video engine (không tắt engine toàn cục).
    /// </summary>
    public void Stop()
    {
        SharedAdsVideoEngine.Instance.FrameAvailable -= OnFrameAvailable;
        InvalidateVisual();
    }

    public void Dispose()
    {
        SharedAdsVideoEngine.Instance.FrameAvailable -= OnFrameAvailable;
        GC.SuppressFinalize(this);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        var localBounds = new Rect(bounds.Size);
        var frame = SharedAdsVideoEngine.Instance.CurrentFrame;

        if (frame == null || frame.PixelSize.Width <= 0 || frame.PixelSize.Height <= 0)
        {
            context.FillRectangle(Brushes.Black, localBounds);
            return;
        }

        var bmpW = (double)frame.PixelSize.Width;
        var bmpH = (double)frame.PixelSize.Height;

        Rect destRect;
        if (Stretch == Stretch.None)
        {
            destRect = new Rect(0, 0, bmpW, bmpH);
        }
        else if (Stretch == Stretch.Uniform)
        {
            double scale = Math.Min(localBounds.Width / bmpW, localBounds.Height / bmpH);
            double w = bmpW * scale;
            double h = bmpH * scale;
            destRect = new Rect((localBounds.Width - w) / 2, (localBounds.Height - h) / 2, w, h);
        }
        else
        {
            destRect = localBounds;
        }

        context.DrawImage(frame, new Rect(0, 0, bmpW, bmpH), destRect);
    }
}
