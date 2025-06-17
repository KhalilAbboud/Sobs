using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sobs.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        [ObservableProperty]
        private bool _isPaneOpen = false;

        [ObservableProperty]
        private ViewModelBase _currentPage = new HomePageViewModel();

        [ObservableProperty]
        private ListItemTemplate? _selectedListItem;

        partial void OnSelectedListItemChanged(ListItemTemplate? value)
        {
            if (value is null) return;
            var instance = Activator.CreateInstance(value.ModelType);
            if (instance is null) return;
            CurrentPage = (ViewModelBase)instance;
        }

        public ObservableCollection<ListItemTemplate> Items { get; } = new()
        {
            new ListItemTemplate(typeof(HomePageViewModel), "HomeRegular", "Dashboard"),
            new ListItemTemplate(typeof(ButtonPageViewModel), "CursorHoverOffRegular", "Task Maker Tool"),
            new ListItemTemplate(typeof(DbConnectivityViewModel), "DatabaseRegular", "DB Verification"),

        };

        [RelayCommand]
        private void TriggerPane()
        {
            IsPaneOpen = !IsPaneOpen;
        }

        [RelayCommand]
        private void NavigateToDbConnectivity()
        {
            CurrentPage = new DbConnectivityViewModel();
        }
                
    }

    public class ListItemTemplate
    {
        public ListItemTemplate(Type type, string iconkey, string label)
        {
            ModelType = type;
            Label = label;
            Application.Current!.TryFindResource(iconkey, out var res);
            ListItemIcon = (StreamGeometry)res!;
        }

        public string Label { get; }
        public Type ModelType { get; }
        public StreamGeometry ListItemIcon { get; }
    }
}
