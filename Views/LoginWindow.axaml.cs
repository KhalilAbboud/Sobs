using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sobs.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
