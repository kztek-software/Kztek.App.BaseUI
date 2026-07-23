using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KztekPreviewAvalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedPageIndex = 0;

    [ObservableProperty]
    private string _pageTitle = "Buttons";

    private static readonly string[] PageTitles =
    {
        "Buttons",
        "Inputs",
        "Badges",
        "Cards",
        "Numeric",
        "Tables",
        "Form Controls",
        "Keyboard (Telex)",
        "Menu & Device Tree",
    };

    partial void OnSelectedPageIndexChanged(int value)
    {
        if (value >= 0 && value < PageTitles.Length)
            PageTitle = PageTitles[value];
    }
}
