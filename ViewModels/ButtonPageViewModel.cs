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
    
        [RelayCommand]
        private async Task ShowMessage()
        {
            var window = TryFindMainWindow();
            if (window is not null)
            {
                var messageWindow = new MessageWindow("It works!");
                await messageWindow.ShowDialog(window);
            }
        }

        [RelayCommand]
        private async Task CreateFile()
        {
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Sobs");
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, "testfile.txt");
            await File.WriteAllTextAsync(filePath, "so this thing does work");

            
            var window = TryFindMainWindow();
            if (window is not null)
            {
                var messageWindow = new MessageWindow($"File created at:\n{filePath}");
                await messageWindow.ShowDialog(window);
            }
        }

        private Window? TryFindMainWindow()
        {
            return Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime life
                ? life.MainWindow
                : null;
        }

        [ObservableProperty]
        private string projectName = string.Empty;
        [RelayCommand]
        private async Task MakeProject(){
            var window = TryFindMainWindow();
            if(window is not null){
                var projectWindow = new ProjectWizardWindow(projectName); // window for later meh meh
                await projectWindow.ShowDialog(window);
            }
        } 
    }
}
