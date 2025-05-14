using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sobs.ViewModels;
using Avalonia.Interactivity;


namespace Sobs.Views;

public partial class DbConnectivityView : UserControl
{
    public DbConnectivityView()
    {
        InitializeComponent();
    }

    private void OnTestConnectionClick(object sender, RoutedEventArgs e)
    {
    var viewModel = DataContext as DbConnectivityViewModel;
    var authComboBox = this.FindControl<ComboBox>("AuthComboBox");
    string selectedAuthMode = (authComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Windows Authentication";
    viewModel?.TestConnectionManually(selectedAuthMode);
    }
}