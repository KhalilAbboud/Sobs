using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sobs.ViewModels;

namespace Sobs.Views;

public partial class ButtonPageView : UserControl
{
    public ButtonPageView()
    {
        InitializeComponent();
        DataContext = new ButtonPageViewModel();
    }
    
}