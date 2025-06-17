using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Sobs.Models;
using DynamicData;
using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia;
using System.Net;
using System.Net.Mail;



namespace Sobs.ViewModels
{
    public partial class ProjectWizardWindowViewModel : ViewModelBase
    {
        // method constructor
        public ProjectWizardWindowViewModel(string taskName)
        {
            //_connectionString = connectionString;
            TaskName = taskName;

            LoadAllPeople();
            LoadTables();

            ConfirmCommand = new RelayCommand(Confirm);
            DbConfirmCommand = new RelayCommand(DbConfirm);
        }

        private readonly string _connectionString = "Server=NX-MACHINE\\SQLEXPRESS;Database=SobsDB2;Integrated Security=True;TrustServerCertificate=True";

        public ObservableCollection<string> TableOptions { get; } = new();
        public ObservableCollection<PersonWithRole> AllPeopleOptions { get; } = new();

        private void LoadTables()
        {
            TableOptions.Clear();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var tableName = reader.GetString(0);
                    TableOptions.Add(tableName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load tables: " + ex.Message);
            }
        }

        private void LoadAllPeople()
        {
            AllPeopleOptions.Clear();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var names = new List<string>();

                // Load from managers
                using (var cmd = new SqlCommand("SELECT Name FROM dbo.managers", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AllPeopleOptions.Add(new PersonWithRole
                        {
                            Name = reader.GetString(0),
                            Role = "Manager"
                        });
                    }
                }

                // Load from supervisors
                using (var cmd = new SqlCommand("SELECT Name FROM dbo.Supervisors", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AllPeopleOptions.Add(new PersonWithRole
                        {
                            Name = reader.GetString(0),
                            Role = "Supervisor"
                        });
                    }
                }

                // Load from employees
                using (var cmd = new SqlCommand("SELECT Name FROM dbo.employees", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AllPeopleOptions.Add(new PersonWithRole
                        {
                            Name = reader.GetString(0),
                            Role = "Employee"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load people: " + ex.Message);
            }
        }

        // for all elements of the tables (managers, supervisors, employees) etc
        [ObservableProperty]
        private PersonWithRole selectedPerson;
        public string SelectedPersonDisplay =>
        SelectedPerson == null
            ? ""
            : $"Selected: [{SelectedPerson.Role}] {SelectedPerson.Name}";

        // the fixed texting box
        [ObservableProperty]
        private string taskNotes = string.Empty;
        
        // obviously the name of the task
        [ObservableProperty]
        private string taskName = string.Empty;

        private string _selectedTable = string.Empty;
        public string SelectedTable
        {
            get => _selectedTable;
            set => SetProperty(ref _selectedTable, value);
        }

        [ObservableProperty]
        private DateTimeOffset? _startTime;
        
        [ObservableProperty]
        private DateTimeOffset? _endTime;
        public DateTimeOffset Today => DateTimeOffset.Now.Date;

        [ObservableProperty]
        private string? startTimeWarning;
        partial void OnStartTimeChanged(DateTimeOffset? value)
        {                     
            if (!value.HasValue)
            {
                StartTimeWarning = "Start time is required";
            }

            else if (value.Value.Date < DateTimeOffset.Now.Date)
            {
                // Reject invalid date "past dates" so to -> (reset or keep old)
                StartTimeWarning = "Time Travel isn't real yet, please select a future date";
            }

            else
            {
                StartTimeWarning = null; // Clear the warning if the date is valid and real
            }

            OnPropertyChanged(nameof(CanConfirm));
            OnEndTimeChanged(EndTime); // Check end time validity AGAIN ! "when start time changes"
        }

        [ObservableProperty]
        private string? endTimeWarning;
        partial void OnEndTimeChanged(DateTimeOffset? value)
        {
            if (!value.HasValue)
            {
                EndTimeWarning = "End time is a must";
            }

            else if (StartTime.HasValue && value.Value.Date < StartTime.Value.Date)
            {
                // Reject invalid date "past dates" so to -> (reset or keep old)
                EndTimeWarning = "End time cannot be before start time";
            }

            else if (value.Value.Date < DateTimeOffset.Now.Date)
            {
                // Reject invalid date "past dates" so to -> (reset or keep old)
                EndTimeWarning = "End time cannot be in the past";
            }

            else
            {
                EndTimeWarning = null;
            }
            
            OnPropertyChanged(nameof(CanConfirm));
        }

        [ObservableProperty]
        private bool generateCodeDocumentation;

        [ObservableProperty]
        private bool generateTaskReport;


        public bool CanConfirm => string.IsNullOrWhiteSpace(StartTimeWarning)
                               && string.IsNullOrWhiteSpace(EndTimeWarning)
                               && EndTime.HasValue
                               && StartTime.HasValue;

        [ObservableProperty]
        private string scopeAndBoundaries = string.Empty;

        [ObservableProperty]
        private string constraintsAndRisks = string.Empty;

        // for the priority sound like UI
        [ObservableProperty]
        private double priorityLevel = 20;

        public IRelayCommand ConfirmCommand { get; }
        public IRelayCommand DbConfirmCommand { get; }


        // for the prioroty changes:
        public string PriorityLevelName => PriorityLevel switch
        {
            <= 20 => "Low",
            <= 40 => "Moderate",
            <= 60 => "High",
            _ => "Crucial"
        };// actual naming
        public IBrush PriorityLevelColor => PriorityLevel switch
        {
            <= 20 => Brushes.LightGreen,
            <= 40 => Brushes.Orange,
            <= 60 => Brushes.Red,
            _ => Brushes.DarkRed
        };// fancy coloring (like why not)
        partial void OnPriorityLevelChanged(double value)
        {
            OnPropertyChanged(nameof(PriorityLevelName));
            OnPropertyChanged(nameof(PriorityLevelColor));
        }

        public void DbConfirm()
        {
            var connectionString = @"yourdb";
            var sql = @"
                INSERT INTO dbo.Tasks
                    ([Name]
                    ,[Responsible]
                    ,[ScopeAndBoundaries]
                    ,[ConstraintsAndRisks]
                    ,[Priority]
                    ,[StartDate]
                    ,[EndDate]
                    ,[IsDocumented]
                    ,[IsPresented])
                VALUES
                    (@Name
                    ,@Responsible
                    ,@Scope
                    ,@Constraints
                    ,@Priority
                    ,@StartDate
                    ,@EndDate
                    ,@IsDocumented
                    ,@IsPresented);
                ";

            try
            {
                using var dbconnection = new SqlConnection(connectionString);
                dbconnection.Open();

                using var cmd = new SqlCommand(sql, dbconnection)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@Name", TaskName);
                cmd.Parameters.AddWithValue("@Responsible", SelectedPerson?.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Scope", string.IsNullOrWhiteSpace(ScopeAndBoundaries)
                    ? (object)DBNull.Value
                    : ScopeAndBoundaries);
                cmd.Parameters.AddWithValue("@Constraints", string.IsNullOrWhiteSpace(ConstraintsAndRisks)
                    ? (object)DBNull.Value
                    : ConstraintsAndRisks);

                cmd.Parameters.AddWithValue("@Priority", PriorityLevelName);

                if (StartTime.HasValue)
                    cmd.Parameters.AddWithValue("@StartDate", StartTime.Value.Date);
                else
                    cmd.Parameters.AddWithValue("@StartDate", DBNull.Value);

                if (EndTime.HasValue)
                    cmd.Parameters.AddWithValue("@EndDate", EndTime.Value.Date);
                else
                    cmd.Parameters.AddWithValue("@EndDate", DBNull.Value);

                cmd.Parameters.AddWithValue("@IsDocumented", GenerateCodeDocumentation ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsPresented", GenerateTaskReport ? 1 : 0);

                // Execute the command and catch anything not so cool, errors savinf, errors loading, etc...

                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 1)
                {
                    var dlg = new Window
                    {
                        Title = "Success",
                        Width = 300,
                        Height = 150,
                        Content = new TextBlock
                        {
                            Text = "Task saved to database.",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Thickness(20)
                        }
                    };

                    if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        dlg.ShowDialog(desktop.MainWindow);
                }
                else
                {
                    var dlg = new Window
                    {
                        Title = "Warning",
                        Width = 300,
                        Height = 150,
                        Content = new TextBlock
                        {
                            Text = $"No rows were inserted (rowsAffected = {rowsAffected}).",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Thickness(20)
                        }
                    };

                    if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        dlg.ShowDialog(desktop.MainWindow);
                }

            }
            catch (Exception ex)
            {
                var dlg = new Window
                    {
                        Title = "Database Error",
                        Width = 400,
                        Height = 200,
                        Content = new TextBlock
                        {
                            Text = $"Error inserting into database:\n{ex.Message}",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Thickness(20)
                        }
                    };

                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    dlg.ShowDialog(desktop.MainWindow);
            }
        }


        // ------------------------------output section------------------------------ //

        //email searching for each person in the db
        private string? GetEmailForPerson(string name)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                const string sql = @"SELECT email FROM managers WHERE name = @Name
                                    UNION
                                    SELECT email FROM supervisors WHERE name = @Name
                                    UNION
                                    SELECT email FROM employees WHERE name = @Name";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", name);

                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[email lookup failed smh] {ex.Message}");
                return null;
            }
        }

        //sending the email to the person method => get it done within the confirm button method
        private void SendEmailWithAttachment(string toEmail, string attachmentPath)
        {
            try
            {
                var fromAddress = new MailAddress("sendingemail", "SOBS App");
                var toAddress = new MailAddress(toEmail);
                const string subject = "New Task Details";
                const string body = "Hello, please find attached the task you are assigned to.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("sendingemail", "awnv tyye iuoa awsd")
                    //this 16 digit code is generated in myaccount.google
                    // the passkey always should get cycled back after each time testing the application, so to avoid any issues with the email sending
                    // and the email is not a real one, it is just a dummy email for testing purposes
                };


                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                };
                message.Attachments.Add(new Attachment(attachmentPath));

                smtp.Send(message);
                // for debug (testing purposes)
                Debug.WriteLine("Email sent to " + toEmail);
            }
            catch (SmtpException smtpEx)
            {
                Debug.WriteLine($"[SMTP ERROR] {smtpEx.StatusCode}: {smtpEx.Message}");
                if (smtpEx.InnerException != null)
                {
                    Debug.WriteLine($"[Inner] {smtpEx.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                // for debug (testing purposes)
                Debug.WriteLine($"[email send fail] {ex.Message}");
            }
        }


        public void Confirm()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var safeTaskName = string.IsNullOrWhiteSpace(TaskName) ? "UntitledTask" : TaskName; // <-- to handle empty task name (trolling)
            var fileName = $"{safeTaskName}_logfile.txt";
            var filePath = Path.Combine(documentsPath, fileName);

            var sb = new System.Text.StringBuilder();
            // sb is stringbuilder shortened, tada !

            sb.AppendLine($"--Task Name: {TaskName}");
            sb.AppendLine();

            sb.AppendLine("--Task Description:");
            sb.AppendLine(string.IsNullOrWhiteSpace(TaskNotes)
                ? "  (none provided)"
                : $"  {TaskNotes}");
            sb.AppendLine();

            // Selected person
            if (SelectedPerson != null)
            {
                sb.AppendLine($"--Task Assigned To => [{SelectedPerson.Role}] {SelectedPerson.Name}");
            }
            else
            {
                sb.AppendLine("=> Oh wait, you forgot to select a person!, you think this taks will do itself :skull:");
            }
            sb.AppendLine();

            // Scope and Boundaries
            sb.AppendLine("--Scope and Boundaries:");
            sb.AppendLine(string.IsNullOrWhiteSpace(ScopeAndBoundaries)
                ? "  (none provided) surely you have something to say ?"
                : $"  {ScopeAndBoundaries}");
            sb.AppendLine();

            // Constraints and Risks
            sb.AppendLine("--Constraints and Risks:");
            sb.AppendLine(string.IsNullOrWhiteSpace(ConstraintsAndRisks)
                ? "  (none provided, really ?)"
                : $"  {ConstraintsAndRisks}");
            sb.AppendLine();

            // Priority Level (both numeric and name)
            sb.AppendLine($"--Priority Level: {PriorityLevelName} ");
            sb.AppendLine();

            // Start Date
            if (StartTime.HasValue)
            {
                sb.AppendLine($"->Start Date: {StartTime.Value:dd-MM-yyyy}");
            }
            else
            {
                sb.AppendLine("->Start Date: (not set yet)");
            }

            // End Date
            if (EndTime.HasValue)
            {
                sb.AppendLine($"->End Date:   {EndTime.Value:dd-MM-yyyy}");
            }
            else
            {
                sb.AppendLine("->End Date:   (not set yet)");
            }
            sb.AppendLine();

            // Report options (checkboxes)
            // If no person was selected, use “(no one)”—otherwise show their name
            var personName = SelectedPerson != null ? SelectedPerson.Name : "(no one for now)"; //<-- to handle the case of no person selected (trolling times two)
            sb.AppendLine($"-Should {personName} write the Code's Documentation?");
            sb.AppendLine(GenerateCodeDocumentation ? "Absolutely" : "No Need For It");
            sb.AppendLine();

            sb.AppendLine($"-Should {personName} generate the Task's Report?");
            sb.AppendLine(GenerateTaskReport ? "Absolutely" : "No Need For It");

            // Timestamp of the log file creation
            sb.AppendLine();
            sb.AppendLine($"Logged on: {DateTimeOffset.Now:dd-MM-yyyy HH:mm:ss}");

            // "tyr/catch block" to Write to the file
            // all the "DEbug.Writeline" are for debugging purpose, i am jsut sick of manually seeing the problem
            try
            {
                File.WriteAllText(filePath, sb.ToString());
                Debug.WriteLine("[debug] Log file written.");
                if (SelectedPerson != null)
                {
                    Debug.WriteLine("[debug] Selected person: " + SelectedPerson.Name);
                    var email = GetEmailForPerson(SelectedPerson.Name);
                    Debug.WriteLine("[debug] Fetched email: " + email);
                    if (!string.IsNullOrEmpty(email))
                    {
                        SendEmailWithAttachment(email, filePath);
                        Debug.WriteLine("[debug] Called SendEmailWithAttachment");
                    }
                    else
                    {
                        Debug.WriteLine("[warning] Could not find email for: " + SelectedPerson.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any inout/output stuff yadayada
                Debug.WriteLine("logfile issues: ");
                Debug.WriteLine($"more details -> {ex.Message}");
                Debug.WriteLine("[exception] Log or email error: " + ex.Message);
            }
        }
    }
}