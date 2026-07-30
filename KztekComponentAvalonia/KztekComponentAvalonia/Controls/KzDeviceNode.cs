using Avalonia.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KztekComponentAvalonia.Theme;

namespace KztekComponentAvalonia.Controls;

/// <summary>Observable data node for KzDeviceTree.</summary>
public class KzDeviceNode : INotifyPropertyChanged
{
    private string         _name       = "";
    private KzDeviceType   _deviceType = KzDeviceType.Generic;
    private KzDeviceStatus _status     = KzDeviceStatus.Unknown;
    private string         _ipAddress  = "";
    private string         _deviceId   = "";
    private bool           _isExpanded;
    private int            _slotTotal     = 0;
    private int            _slotAvailable = 0;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public KzDeviceType DeviceType
    {
        get => _deviceType;
        set { _deviceType = value; OnPropertyChanged(); OnPropertyChanged(nameof(DeviceIcon)); OnPropertyChanged(nameof(NameBrush)); }
    }

    public KzDeviceStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DeviceIcon));
            OnPropertyChanged(nameof(DeviceIconBrush));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusLabel));
            OnPropertyChanged(nameof(StatusIcon));
            OnPropertyChanged(nameof(StatusBadgeBackground));
        }
    }

    public string IpAddress
    {
        get => _ipAddress;
        set { _ipAddress = value; OnPropertyChanged(); }
    }

    public string DeviceId
    {
        get => _deviceId;
        set { _deviceId = value; OnPropertyChanged(); }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    public int SlotTotal
    {
        get => _slotTotal;
        set { _slotTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(SlotText)); OnPropertyChanged(nameof(SlotBrush)); OnPropertyChanged(nameof(HasSlots)); OnPropertyChanged(nameof(NameBrush)); }
    }

    public int SlotAvailable
    {
        get => _slotAvailable;
        set { _slotAvailable = value; OnPropertyChanged(); OnPropertyChanged(nameof(SlotText)); OnPropertyChanged(nameof(SlotBrush)); OnPropertyChanged(nameof(NameBrush)); }
    }

    public ObservableCollection<KzDeviceNode> Children { get; } = new();

    public KzDeviceNode()
    {
        Children.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasChildren));
    }

    /// <summary>Whether this node has child nodes to expand — independent of DeviceType.</summary>
    public bool HasChildren => Children.Count > 0;

    // ── Computed display properties (used in AXAML bindings) ──────────────────

    public string DeviceIcon => DeviceType switch
    {
        KzDeviceType.Group      => "📁",
        // Vẫn là ý nghĩa "group" (không phải thiết bị Gate vật lý) — dùng icon folder khác (mở) để
        // phân biệt trực quan với group thường, không dùng icon cổng/barrier gây hiểu nhầm là thiết bị.
        KzDeviceType.GateGroup  => "📂",
        KzDeviceType.Camera     => Status switch
        {
            KzDeviceStatus.Offline => "🚫",
            KzDeviceStatus.Warning => "⚠",
            _                      => "📷",
        },
        KzDeviceType.Sensor     => Status switch
        {
            KzDeviceStatus.Online  => "🅿",
            KzDeviceStatus.Offline => "🚗",
            KzDeviceStatus.Warning => "⚠",
            _                      => "○",
        },
        KzDeviceType.Controller => "🖥",
        KzDeviceType.EntryGate  => "🚗",
        KzDeviceType.ExitGate   => "🚪",
        KzDeviceType.Server     => "🖧",
        KzDeviceType.Network    => "🌐",
        _                       => "⚙",
    };

    public string SlotText => _slotTotal > 0 ? $"{_slotAvailable}/{_slotTotal}" : "";
    public bool   HasSlots => _slotTotal > 0;

    public IBrush SlotBrush => _slotTotal <= 0
        ? new SolidColorBrush(Color.Parse("#9CA3AF"))
        : _slotAvailable == 0 ? new SolidColorBrush(Color.Parse("#EF4444"))
        : _slotAvailable == 1 ? new SolidColorBrush(Color.Parse("#F59E0B"))
        : new SolidColorBrush(Color.Parse("#22C55E"));

    public IBrush NameBrush => DeviceType == KzDeviceType.Camera && _slotTotal > 0
        ? SlotBrush
        : new SolidColorBrush(Color.Parse("#1A1A1A"));

    public IBrush DeviceIconBrush => Status == KzDeviceStatus.Warning
        ? new SolidColorBrush(Color.Parse("#F05922"))
        : new SolidColorBrush(Colors.Black);

    public string StatusColor => Status switch
    {
        KzDeviceStatus.Online  => "#22C55E",
        KzDeviceStatus.Offline => "#EF4444",
        KzDeviceStatus.Warning => "#F59E0B",
        _                      => "#9CA3AF",
    };

    public string StatusBadgeBackground => Status switch
    {
        KzDeviceStatus.Online  => "#DCFCE7",
        KzDeviceStatus.Offline => "#FEE2E2",
        KzDeviceStatus.Warning => "#FEF3C7",
        _                      => "#F3F4F6",
    };

    public string StatusIcon => Status switch
    {
        KzDeviceStatus.Online  => "●",
        KzDeviceStatus.Offline => "●",
        KzDeviceStatus.Warning => "⚠",
        _                      => "○",
    };

    public string StatusLabel => Status switch
    {
        KzDeviceStatus.Online  => "Online",
        KzDeviceStatus.Offline => "Offline",
        KzDeviceStatus.Warning => "Cảnh báo",
        _                      => "Không xác định",
    };

    public bool IsGroup => DeviceType is KzDeviceType.Group or KzDeviceType.GateGroup;
    public bool IsLeaf  => !IsGroup;

    // ── INotifyPropertyChanged ────────────────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
