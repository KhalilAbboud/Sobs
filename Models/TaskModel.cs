using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sobs.Models
{
    public class TaskModel : ObservableObject
    {
        public int TaskID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Responsible { get; set; } = string.Empty;
        public string ScopeAndBoundaries { get; set; } = string.Empty;
        public string ConstraintsAndRisks { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // “Low”/“Moderate”/“High”/“Crucial”

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public bool IsDocumented { get; set; }
        public bool IsPresented { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        
        public TaskModel Clone()
        {
            return new TaskModel
            {
                TaskID = this.TaskID,
                Name = this.Name,
                Responsible = this.Responsible,
                ScopeAndBoundaries = this.ScopeAndBoundaries,
                ConstraintsAndRisks = this.ConstraintsAndRisks,
                Priority = this.Priority,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                IsDocumented = this.IsDocumented,
                IsPresented = this.IsPresented
            };
        }

    }
}
