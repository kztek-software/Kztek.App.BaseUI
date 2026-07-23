using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Threading;
using KztekComponentAvalonia.Controls;

namespace KztekPreviewAvalonia.Pages;

public class ProductRow
{
    public string Code   { get; set; } = string.Empty;
    public string Name   { get; set; } = string.Empty;
    public string Price  { get; set; } = string.Empty;
    public int    Qty    { get; set; }
    public string Status { get; set; } = string.Empty;
}

public partial class SidebarPage : UserControl
{
    private static readonly List<ProductRow> _gridData = new()
    {
        new() { Code = "KZ-4K01",   Name = "Camera IP Ngoài Trời 4K",  Price = "2.500.000", Qty = 150, Status = "Còn hàng" },
        new() { Code = "KZ-DVR16",  Name = "Bộ Ghi Hình DVR 16 Kênh",  Price = "4.200.000", Qty = 48,  Status = "Còn hàng" },
        new() { Code = "KZ-NVR08",  Name = "Bộ Ghi Hình NVR 8 Kênh",   Price = "3.800.000", Qty = 0,   Status = "Hết hàng" },
        new() { Code = "KZ-DOME01", Name = "Camera Dome Hỗ Trợ AI",     Price = "1.800.000", Qty = 200, Status = "Còn hàng" },
        new() { Code = "KZ-PTZ04",  Name = "Camera PTZ Zoom 36x",       Price = "8.500.000", Qty = 5,   Status = "Sắp hết"  },
        new() { Code = "KZ-HDD4T",  Name = "Ổ Cứng Surveillance 4TB",  Price = "2.100.000", Qty = 90,  Status = "Còn hàng" },
        new() { Code = "KZ-SW08",   Name = "Switch PoE 8 Cổng",         Price = "1.200.000", Qty = 75,  Status = "Còn hàng" },
    };

    public SidebarPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetupSidebar();
        SetupDataGrid();
    }

    private void SetupSidebar()
    {
        if (this.FindControl<KzSidebar>("DemoSidebar") is not { } sidebar) return;
        sidebar.Items = new List<KzSidebarItemData>
        {
            new() { Label = "Tổng quan",  Icon = "📊" },
            new() { Label = "Sản phẩm",   Icon = "📦" },
            new() { Label = "Đơn hàng",   Icon = "🛒" },
            new() { Label = "Khách hàng", Icon = "👥" },
            new() { Label = "Báo cáo",    Icon = "📈" },
            new() { Label = "Cài đặt",    Icon = "⚙️" },
        };
        sidebar.SelectedIndex = 0;
        sidebar.ItemSelected += (_, data) =>
        {
            if (this.FindControl<TextBlock>("SidebarInfoText") is { } tb && data is not null)
                tb.Text = $"Đang xem: {data.Label}";
        };
    }

    private void SetupDataGrid()
    {
        if (this.FindControl<DataGrid>("SampleGrid") is not { } grid) return;
        grid.ItemsSource = new List<ProductRow>
        {
            new() { Code = "KZ-4K01",   Name = "Camera IP Ngoài Trời 4K",   Price = "2.500.000", Qty = 150, Status = "Còn hàng" },
            new() { Code = "KZ-DVR16",  Name = "Bộ Ghi Hình DVR 16 Kênh",   Price = "4.200.000", Qty = 48,  Status = "Còn hàng" },
            new() { Code = "KZ-NVR08",  Name = "Bộ Ghi Hình NVR 8 Kênh",    Price = "3.800.000", Qty = 0,   Status = "Hết hàng" },
            new() { Code = "KZ-DOME01", Name = "Camera Dome Hỗ Trợ AI",      Price = "1.800.000", Qty = 200, Status = "Còn hàng" },
            new() { Code = "KZ-PTZ04",  Name = "Camera PTZ Zoom 36x",        Price = "8.500.000", Qty = 5,   Status = "Sắp hết" },
            new() { Code = "KZ-HDD4T",  Name = "Ổ Cứng Surveillance 4TB",   Price = "2.100.000", Qty = 90,  Status = "Còn hàng" },
            new() { Code = "KZ-SW08",   Name = "Switch PoE 8 Cổng",          Price = "1.200.000", Qty = 75,  Status = "Còn hàng" },
        };
    }
}
