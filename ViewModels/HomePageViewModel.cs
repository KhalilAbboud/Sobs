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
using System.Linq;

namespace Sobs.ViewModels
{
    public partial class HomePageViewModel : ViewModelBase
    {
        // connection string to the database
        private readonly string _connectionString =
            @"yourdb";

        public ObservableCollection<TaskModel> Tasks { get; } = new();
        public IRelayCommand<TaskModel> SaveTaskCommand { get; }
        
        [ObservableProperty]
        private TaskModel? _selectedTask;

        public IRelayCommand ReloadCommand { get; }
        public IRelayCommand DeleteCommand { get; }

        public HomePageViewModel()
        {
            // Wire up commands of loading, deleting and editing if a task is selected
            ReloadCommand = new RelayCommand(LoadTasks);
            DeleteCommand = new RelayCommand(DeleteSelected, () => SelectedTask != null);
            SaveTaskCommand = new RelayCommand<TaskModel>(SaveTask);

            // Load immediately on startup of the application
            LoadTasks();
        }

        public ObservableCollection<string> PriorityOptions { get; } = new()
        {
            "Low", "Moderate", "High", "Crucial"
        };

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
                    ORDER BY TaskID DESC;
                ";

                using var cmd = new SqlCommand(sql, conn)
                {
                    CommandType = CommandType.Text
                };

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

            // Ensure DeleteCommand.CanExecute is re‐evaluated
            DeleteCommand.NotifyCanExecuteChanged();
        }

        private void DeleteSelected()
        {
            if (SelectedTask is null)
                return;

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                const string sql = "DELETE FROM dbo.Tasks WHERE TaskID = @Id";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", SelectedTask.TaskID);
                var rows = cmd.ExecuteNonQuery();

                if (rows == 1)
                {
                    Tasks.Remove(SelectedTask);
                    SelectedTask = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete failed: {ex.Message}");
            }

            DeleteCommand.NotifyCanExecuteChanged();
        }

        private void SaveTask(TaskModel? task)
        {
            if (task is null)
                return;

            UpdateTaskInDatabase(task);
            UpdateTaskInMemory(task);
        }

        //Update edited task in db
        private void UpdateTaskInDatabase(TaskModel updated)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                const string sql = @"
                    UPDATE dbo.Tasks SET
                        Name = @Name,
                        Responsible = @Responsible,
                        ScopeAndBoundaries = @Scope,
                        ConstraintsAndRisks = @Risks,
                        Priority = @Priority,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        IsDocumented = @Doc,
                        IsPresented = @Pres
                    WHERE TaskID = @Id";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", updated.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@Responsible", updated.Responsible ?? string.Empty);
                cmd.Parameters.AddWithValue("@Scope", updated.ScopeAndBoundaries ?? string.Empty);
                cmd.Parameters.AddWithValue("@Risks", updated.ConstraintsAndRisks ?? string.Empty);
                cmd.Parameters.AddWithValue("@Priority", updated.Priority ?? string.Empty);
                cmd.Parameters.AddWithValue("@StartDate", (object?)updated.StartDate?.DateTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object?)updated.EndDate?.DateTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Doc", updated.IsDocumented);
                cmd.Parameters.AddWithValue("@Pres", updated.IsPresented);
                cmd.Parameters.AddWithValue("@Id", updated.TaskID);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update failed: {ex.Message}");
            }
        }

        // Update edited task in memory of the application
        private void UpdateTaskInMemory(TaskModel updated)
        {
            var original = Tasks.FirstOrDefault(t => t.TaskID == updated.TaskID);
            if (original != null)
            {
                // Update properties manually
                original.Name = updated.Name;
                original.Responsible = updated.Responsible;
                original.ScopeAndBoundaries = updated.ScopeAndBoundaries;
                original.ConstraintsAndRisks = updated.ConstraintsAndRisks;
                original.Priority = updated.Priority;
                original.StartDate = updated.StartDate;
                original.EndDate = updated.EndDate;
                original.IsDocumented = updated.IsDocumented;
                original.IsPresented = updated.IsPresented;
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

            // Notify DeleteCommand that CanExecute may have changed
            DeleteCommand.NotifyCanExecuteChanged();
        }

    }
}
