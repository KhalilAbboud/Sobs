using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using Sobs.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;

namespace Sobs.ViewModels
{
    public partial class UserDashboardViewModel : ViewModelBase
    {
        private readonly string _currentUser;

        // connection string to the database
        private readonly string _connectionString =
            @"yourdb";

        public ObservableCollection<TaskModel> Tasks { get; } = new();
        
        [ObservableProperty]
        private TaskModel? _selectedTask;

        public IRelayCommand ReloadCommand { get; }

        public UserDashboardViewModel(string currentUser)
        {
            // Wire up commands of loading and deleting if a task is selected
            _currentUser = currentUser; //to only load the user's logged in tasks
            ReloadCommand = new RelayCommand(LoadTasks);

            // Load immediately on startup of the application
            LoadTasks();
        }

        private void LoadTasks()
        {
            Tasks.Clear();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                const string sql = @"
                    SELECT TaskID
                         , Name
                         , Responsible
                         , ScopeAndBoundaries
                         , ConstraintsAndRisks
                         , Priority
                         , StartDate
                         , EndDate
                         , IsDocumented
                         , IsPresented
                    FROM dbo.Tasks
                    WHERE LTRIM(RTRIM(LOWER(Responsible))) = LTRIM(RTRIM(LOWER(@UserName)))
                    ORDER BY TaskID DESC;
                ";

                using var cmd = new SqlCommand(sql, conn)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.AddWithValue("@UserName", _currentUser);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var item = new TaskModel
                    {
                        TaskID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Responsible = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        ScopeAndBoundaries = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        ConstraintsAndRisks = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        Priority = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        StartDate = reader.IsDBNull(6) ? null : new DateTimeOffset(reader.GetDateTime(6)),
                        EndDate = reader.IsDBNull(7) ? null : new DateTimeOffset(reader.GetDateTime(7)),
                        IsDocumented = !reader.IsDBNull(8) && reader.GetBoolean(8),
                        IsPresented = !reader.IsDBNull(9) && reader.GetBoolean(9),
                    };

                    Tasks.Add(item);
                }

                System.Diagnostics.Debug.WriteLine($"[LoadTasks] Loaded {Tasks.Count} tasks.");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load failed: {ex.Message}");
                var dlg = new Window
                {
                    Title = "Error loading tasks",
                    Width = 400,
                    Height = 200,
                    Content = new TextBlock
                    {
                        Text = ex.Message,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(20)
                    }
                };
                dlg.Show();
            }
        }


        partial void OnSelectedTaskChanged(TaskModel? oldValue, TaskModel? newValue)
        {
            // Un‐select a task if it was previously selected
            if (oldValue != null)
                oldValue.IsSelected = false;

            // Select a task
            if (newValue != null)
                newValue.IsSelected = true;
        }

    }
}
