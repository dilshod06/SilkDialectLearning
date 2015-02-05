using MahApps.Metro;
using SilkDialectLearning.Flyouts;
using SilkDialectLearning.DAL;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SilkDialectLearning.Navigation
{
    /// <summary>
    /// Interaction logic for UnitsPage.xaml
    /// </summary>
    public partial class UnitsPage : Page
    {
        public MainViewModel MainViewModel { get; set; }
        public HomeFlyout HomeFlyout { get; set; }
        public UnitsPage(HomeFlyout homeFlyout, MainViewModel mainViewModel)
        {
            this.HomeFlyout = homeFlyout;
            this.MainViewModel = mainViewModel;
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
            var a = this.Resources;
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


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Unit unit = e.AddedItems[0] as Unit;
                MainViewModel.ViewModel.SelectedUnit = unit;
                this.HomeFlyout.Navigate(new LessonsPage(this.HomeFlyout, MainViewModel));
            }
        }
    }
}
