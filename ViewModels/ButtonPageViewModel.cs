using System;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Sobs.Views;
using Sobs.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sobs.ViewModels
{
    public partial class ButtonPageViewModel : ViewModelBase
    {

        private Window? TryFindMainWindow()
        {
            return Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime life
                ? life.MainWindow
                : null;
        }


        //private readonly DbConnectivityViewModel _dbConnectivityViewModel;

        [ObservableProperty]
        private string taskName = string.Empty;
        [RelayCommand]
        private async Task MakeTask(){
            var window = TryFindMainWindow();
            if(window is not null){
                if (!App.IsDatabaseConnected)
                {
                    await ShowErrorAsync("Verify your Database Connection Credentials first.");
                    return;
                }


                if (string.IsNullOrWhiteSpace(TaskName))
                {
                    await ShowErrorAsync("Please enter a Proper Task name.");
                    return;
                }

                var projectWindow = new ProjectWizardWindow(TaskName);               
                await projectWindow.ShowDialog(window);
            }
        }


        private async Task ShowErrorAsync(string message)
        {
            var errorWindow = new Window
            {
                Title = "Error",
                Width = 300,
                Height = 150,
                Content = new TextBlock
                {
                    Text = message,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Thickness(20)
                }
        };
        var mainWindow = TryFindMainWindow();
            if (mainWindow is not null){            
                await errorWindow.ShowDialog(mainWindow);
            }
            else{           
                errorWindow.Show();
            }
        }
        public bool IsDatabaseConnected { get; set; } = false; 
    }
}
