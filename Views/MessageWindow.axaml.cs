using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sobs.Views;

namespace Sobs.Views
{
    public partial class MessageWindow : Window
    {
        public MessageWindow(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void OnOkClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
    }
}
