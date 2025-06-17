using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.ApplicationLifetimes;
using Sobs;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Identity.Client;

namespace Sobs.ViewModels
{
    public partial class DbConnectivityViewModel : ViewModelBase
    {      
        
        // these two for the connection verification
        [ObservableProperty]
        private string connectedDatabaseName = string.Empty;

        [ObservableProperty]
        private bool isConnected;

        //database related stuff
        [ObservableProperty]
        private string serverName = string.Empty;

        [ObservableProperty]
        private string databaseName = string.Empty;

        [ObservableProperty]
        private string selectedAuthMode = "Windows Authentication";

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        // this is for the connection status (just more verification cuz crutial stuff)
        [ObservableProperty]
        private string tableList = string.Empty;

        [ObservableProperty]
        private bool isDbConnected;
        public string DbStatusText => IsDbConnected ? "connected" : "not connected";
        public static bool IsDatabaseConnected { get; private set; } = false;

        // about the first 2 on top of the class
        private RelayCommand ShowTablesCommand { get; } 
        public IRelayCommand ShowTables => ShowTablesCommand;    

        public DbConnectivityViewModel()
        {
            ShowTablesCommand = new RelayCommand(ExecuteShowTables, () => IsConnected);
        }     
        
        private async void ExecuteShowTables()
        {
            // "used for debugging" Console.WriteLine("ShowTables clicked!");
            if (!string.IsNullOrEmpty(TableList))
                await ShowDialog("Tables", TableList);
            else
                await ShowDialog("Tables", "No table information available.");
        }

        public string ConnectionString
        {
            get
            {
                return SelectedAuthMode == "SQL Server Authentication"
                ? $"Server={ServerName};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;"
                : $"Server={ServerName};Database={DatabaseName};Integrated Security=True;TrustServerCertificate=True;";
            }
        }

        // useless but i ll keep it for now
        private bool CanShowTables() => IsConnected;

        private async Task ShowDialog(string title, string message)
        {
            var dialog = new Window
            {
                Width = 400,
                Height = 200,
                Title = title
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0),
            };
            okButton.Click += (_, _) => dialog.Close();

            dialog.Content = new StackPanel
            {
                Margin = new Thickness(10),
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                    okButton
                }
            };

            var mainWindow = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await dialog.ShowDialog(mainWindow);
        }


        // main method for connection testing (there is a lot of shinanigan here so far just bare with me)
        public async void TestConnectionManually(string authMode)
        {
            if (string.IsNullOrWhiteSpace(ServerName) || string.IsNullOrWhiteSpace(DatabaseName))
            {
                await ShowDialog("Error", "Server name and database name are required.");
                return;
            }

            if (authMode == "SQL Server Authentication")
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    await ShowDialog("Error", "Username and password required for SQL Authentication.");
                    return;
                }
            }

            string connectionString = authMode == "SQL Server Authentication"
                ? $"Server={ServerName};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;"
                : $"Server={ServerName};Database={DatabaseName};Integrated Security=True;TrustServerCertificate=True;";

            try
            {
                using SqlConnection connection = new(connectionString);
                await connection.OpenAsync();
                using SqlCommand cmd = new("SELECT name FROM sys.tables", connection);
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                string result = "Connected. Tables:\n";
                while (await reader.ReadAsync())
                {
                    result += reader.GetString(0) + "\n";
                }

                TableList = result;
                IsConnected = true;
                ConnectedDatabaseName = DatabaseName;
                ShowTablesCommand.NotifyCanExecuteChanged();
                await ShowDialog("Success","Connection verification successful!");
                App.IsDatabaseConnected = true;
                
                
            }
            catch (Exception ex)
            {
                await ShowDialog("Connection Failed", ex.Message);
                App.IsDatabaseConnected = false;
            }
        }
    }
}
