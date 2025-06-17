using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Sobs.Views;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;
using Microsoft.Data.SqlClient;
using System;

namespace Sobs.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? password;

        [ObservableProperty]
        private string? errorMessage;

        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password cannot be empty.";
                return;
            }

            var userRole = await CheckCredentialsAsync(Username!, Password!);

            if (string.IsNullOrEmpty(userRole))
            {
                ErrorMessage = "Invalid credentials.";
                return;
            }

            if (userRole == "Admin")
            {
                var apps = Avalonia.Application.Current;
                if (apps != null && apps.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var mainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel()
                    };
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                }
            }
            else
            {
                var userWindow = new UserDashboard(Username!);
                userWindow.Show();
            }

            // Find and close login window
            var loginWindow = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app
                ? app.Windows.FirstOrDefault(w => w is LoginWindow)
                : null;

            loginWindow?.Close();
        }

        private async Task<string?> CheckCredentialsAsync(string username, string password)
        {   
            var connectionString = "yourdb";
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT isAdmin FROM Users WHERE Username = @username AND Password = @password";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    var result = await command.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        bool isAdmin = (bool)result;
                        return isAdmin ? "Admin" : "User";
                    }
                    else
                    {
                        return null; // No user found with the given credentials eh
                    }
                }
            }            
        }
    }
}
