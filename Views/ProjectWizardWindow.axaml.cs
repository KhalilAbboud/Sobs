using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sobs.ViewModels;

namespace Sobs.Views
{
    public partial class ProjectWizardWindow : Window
    {
        public ProjectWizardWindow(string projectName)
        {
            InitializeComponent();
            DataContext = new ProjectWizardWindowViewModel(projectName);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}