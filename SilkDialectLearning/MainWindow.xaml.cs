using MahApps.Metro.Controls;
using SilkDialectLearningBLL;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace SilkDialectLearning
{
    /// <summary>
    /// A reimplementation of NavigationWindow based on MetroWindow.
    /// </summary>

    public partial class MainWindow : MetroWindow
    {
        public MainViewModel MainWindowViewModel;
        public MainWindow()
        {
            MainWindowViewModel = new MainViewModel();
            this.DataContext = MainWindowViewModel;
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
       
        
                
        private void ToggleFlyout(int index)
        {
            var flyout = this.Flyouts.Items[index] as Flyout;
            if (flyout == null)
                return;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleFlyout(0);
        }

        private void MetroWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MainWindowViewModel.ViewModel.SelectedLesson != null && MainWindowViewModel.ViewModel.SceneViewModel.SelectedScene != null)
                ToggleFlyout(1);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleFlyout(2);
        }
    }
}
