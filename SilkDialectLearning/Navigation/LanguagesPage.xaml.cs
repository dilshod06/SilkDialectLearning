using MahApps.Metro;
using SilkDialectLearning.Flyouts;
using SilkDialectLearningBLL;
using SilkDialectLearningDAL;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SilkDialectLearning.Navigation
{
    public partial class LanguagesPage : Page
    {
        public MainViewModel MainViewModel { get; set; }
        public HomeFlyout HomeFlyout { get; set; }
        public LanguagesPage(HomeFlyout HomeFlyout, MainViewModel MainWindowViewModel)
        {
            this.MainViewModel = MainWindowViewModel;
            this.HomeFlyout = HomeFlyout;
            InitializeComponent();
            this.DataContext = this.MainViewModel;
            ThemeManager.IsThemeChanged += ThemeManager_IsThemeChanged;
            AddResourceDictionary();
        }

        private void ThemeManager_IsThemeChanged(object sender, OnThemeChangedEventArgs e)
        {
            if (e.AppTheme.Name == "Dark")
            {
                this.Resources.MergedDictionaries.Clear();
                var rd = new ResourceDictionary
                {
                    Source = new Uri(@"/MahApps.Metro;component/Styles/Accents/BaseLight.xaml", UriKind.RelativeOrAbsolute)
                };
                this.Resources.MergedDictionaries.Add(rd);
            }
            else
            {
                this.Resources.MergedDictionaries.Clear();
                var rd = new ResourceDictionary
                {
                    Source = new Uri(@"/MahApps.Metro;component/Styles/Accents/BaseDark.xaml", UriKind.RelativeOrAbsolute)
                };
                this.Resources.MergedDictionaries.Add(rd);
            }
        }
        private void AddResourceDictionary()
        {
            if (ThemeManager.DetectAppStyle(Application.Current).Item1.Name == "Dark")
            {
                this.Resources.MergedDictionaries.Clear();
                var rd = new ResourceDictionary
                {
                    Source = new Uri(@"/MahApps.Metro;component/Styles/Accents/BaseLight.xaml", UriKind.RelativeOrAbsolute)
                };
                this.Resources.MergedDictionaries.Add(rd);
            }
            else
            {
                this.Resources.MergedDictionaries.Clear();
                var rd = new ResourceDictionary
                {
                    Source = new Uri(@"/MahApps.Metro;component/Styles/Accents/BaseDark.xaml", UriKind.RelativeOrAbsolute)
                };
                this.Resources.MergedDictionaries.Add(rd);
            }
        }

        private void Languages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Language language = e.AddedItems[0] as Language;
                MainViewModel.ViewModel.SelectedLanguage = language;
                this.HomeFlyout.Navigate(new LevelsPage(this.HomeFlyout, this.MainViewModel));
            }
        }
    }
}
