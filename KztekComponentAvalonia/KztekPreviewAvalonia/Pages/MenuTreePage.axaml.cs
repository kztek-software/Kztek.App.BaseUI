using Avalonia.Controls;
using Avalonia.Interactivity;
using KztekComponentAvalonia.Controls;
using KztekComponentAvalonia.Theme;

namespace KztekPreviewAvalonia.Pages;

public partial class MenuTreePage : UserControl
{
    public MenuTreePage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        BuildTree();
    }

    private void BuildTree()
    {
        var tree   = this.FindControl<KzDeviceTree>("DeviceTree");
        var detail = this.FindControl<TextBlock>("DetailText");
        if (tree is null) return;

        // ── Camera group ────────────────────────────────────────────────────
        var grpCam = tree.AddDevice("Camera",           KzDeviceType.Group);
        tree.AddDevice(grpCam, "Camera cổng vào 1",   KzDeviceType.Camera,     KzDeviceStatus.Online,  "192.168.1.10", "CAM-001");
        tree.AddDevice(grpCam, "Camera cổng vào 2",   KzDeviceType.Camera,     KzDeviceStatus.Online,  "192.168.1.11", "CAM-002");
        tree.AddDevice(grpCam, "Camera cổng ra",       KzDeviceType.Camera,     KzDeviceStatus.Warning, "192.168.1.12", "CAM-003");
        tree.AddDevice(grpCam, "Camera khu A tầng B1", KzDeviceType.Camera,     KzDeviceStatus.Offline, "192.168.1.13", "CAM-004");

        // ── Barrier group ───────────────────────────────────────────────────
        var grpGate = tree.AddDevice("Barrier / Gate", KzDeviceType.Group);
        tree.AddDevice(grpGate, "Barrier vào trái",    KzDeviceType.EntryGate,  KzDeviceStatus.Online,  "192.168.1.20", "GTI-001");
        tree.AddDevice(grpGate, "Barrier vào phải",    KzDeviceType.EntryGate,  KzDeviceStatus.Online,  "192.168.1.21", "GTI-002");
        tree.AddDevice(grpGate, "Barrier ra",           KzDeviceType.ExitGate,   KzDeviceStatus.Online,  "192.168.1.22", "GTO-001");

        // ── Controllers ─────────────────────────────────────────────────────
        var grpCtrl = tree.AddDevice("Bộ điều khiển",  KzDeviceType.Group);
        tree.AddDevice(grpCtrl, "Main Controller",     KzDeviceType.Controller, KzDeviceStatus.Online,  "192.168.1.30", "CTRL-001");
        tree.AddDevice(grpCtrl, "Sub Controller",      KzDeviceType.Controller, KzDeviceStatus.Offline, "192.168.1.31", "CTRL-002");

        // ── Infrastructure ──────────────────────────────────────────────────
        var grpInfra = tree.AddDevice("Hạ tầng",       KzDeviceType.Group);
        tree.AddDevice(grpInfra, "Server chính",       KzDeviceType.Server,     KzDeviceStatus.Online,  "192.168.1.100", "SRV-001");
        tree.AddDevice(grpInfra, "Switch tầng 1",      KzDeviceType.Network,    KzDeviceStatus.Online,  "192.168.1.200", "SW-001");
        tree.AddDevice(grpInfra, "Cảm biến vòng 1",   KzDeviceType.Sensor,     KzDeviceStatus.Warning, "192.168.1.50",  "SNS-001");

        // ── Selection handler ───────────────────────────────────────────────
        if (detail is not null)
        {
            tree.NodeSelected += (_, node) =>
            {
                if (node is null) return;
                string statusStr = node.Status switch
                {
                    KzDeviceStatus.Online  => "🟢 Online",
                    KzDeviceStatus.Offline => "🔴 Offline",
                    KzDeviceStatus.Warning => "🟡 Warning",
                    _                      => "⚪ Unknown"
                };
                detail.Text = node.IsGroup
                    ? $"Nhóm: {node.Name}\nSố thiết bị con: {node.Children.Count}"
                    : $"Tên       : {node.Name}\n" +
                      $"Loại      : {node.DeviceType}\n" +
                      $"Trạng thái: {statusStr}\n" +
                      $"IP        : {(string.IsNullOrEmpty(node.IpAddress) ? "—" : node.IpAddress)}\n" +
                      $"Device ID : {(string.IsNullOrEmpty(node.DeviceId)  ? "—" : node.DeviceId)}";
            };
        }
    }
}
