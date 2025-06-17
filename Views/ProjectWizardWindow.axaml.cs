using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sobs.ViewModels;

namespace Sobs.Views
{
    public partial class ProjectWizardWindow : Window
    {
        public ProjectWizardWindow(string taskName)
        {
            InitializeComponent();
            DataContext = new ProjectWizardWindowViewModel(taskName);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
    }
}