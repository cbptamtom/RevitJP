using PresentationFilter.Utilities;
using PresentationFilter.Views;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PresentationFilter.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }


        public ICommand HomeCommand { get; set; }
        public ICommand FilterManagerCommand { get; set; }
        public ICommand DeleteFilterCommand { get; set; }
        public ICommand ApplyFilterCommand { get; set; }



        private void Home(object obj) => CurrentView = new HomeViewModel();
        private void FilterManager(object obj) => CurrentView = new FilterManagerViewModel();
        private void DeleteFilter(object obj) => CurrentView = new DeleteFilterViewModel();
        private void ApplyFilter(object obj) => CurrentView = new ApplyFilterViewModel();

        public MainViewModel()
        {
            HomeCommand = new RelayCommand(Home);
            FilterManagerCommand = new RelayCommand(FilterManager);
            DeleteFilterCommand = new RelayCommand(DeleteFilter);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            CurrentView = new HomeViewModel();
        }
    }
}
