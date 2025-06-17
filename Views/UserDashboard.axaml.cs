using Avalonia.Controls;
using Sobs.ViewModels;

namespace Sobs.Views
{
    public partial class UserDashboard : Window
    {
        public UserDashboard(string currentUser)
        {
            InitializeComponent();
            DataContext = new UserDashboardViewModel(currentUser);
        }
    }
}