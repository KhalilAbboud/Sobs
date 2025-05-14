using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sobs.ViewModels
{
    public partial class ProjectWizardWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string projectName;

        [ObservableProperty]
        private int currentStep = 1;

        public IRelayCommand GoNextCommand { get; }
        public IRelayCommand GoBackCommand { get; }

        public ProjectWizardWindowViewModel(string projectName)
        {
            ProjectName = projectName;

            GoNextCommand = new RelayCommand(GoNext);
            GoBackCommand = new RelayCommand(GoBack);
        }

        public void GoNext()
        {
            if (CurrentStep < 3)
            {
                CurrentStep++;
            }
        }
        public void GoBack()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
        }

        public string StepDescription => $"Step {CurrentStep}: {GetStepText()}";

        partial void OnCurrentStepChanged(int value){
            OnPropertyChanged(nameof(StepDescription));
        }
        
        private string GetStepText(){
            return CurrentStep switch{
                    1 => "Project Info",
                    2 => "Details",
                    3 => "Confirmation",
                    _ => "Unknown Step"
                };
        }
    }
}