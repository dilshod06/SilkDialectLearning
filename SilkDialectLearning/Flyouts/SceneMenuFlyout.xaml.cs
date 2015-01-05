using MahApps.Metro.Controls;
using SilkDialectLearning.Pages;
using System.Windows;
using System.Windows.Controls;

namespace SilkDialectLearning.Flyouts
{
    /// <summary>
    /// Interaction logic for SceneMenuFlyout.xaml
    /// </summary>
    public partial class SceneMenuFlyout : Flyout
    {
        public SceneMenuFlyout()
        {
            InitializeComponent();
            this.Loaded += SceneMenuFlyout_Loaded;
        }
        SceneMenuFlyout sceneMenu;
        MetroWindow metroWindow;
        void SceneMenuFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            sceneMenu = sender as SceneMenuFlyout;
            metroWindow = VisualTreeHelpers.FindAncestor<MetroWindow>(sceneMenu);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var frame = VisualTreeHelpers.FindChild<Frame>(metroWindow);
            var mainViewModel = this.DataContext as MainViewModel;
            frame.Navigate(new EditScenePage(mainViewModel));
        }

        private void Activites_Click(object sender, RoutedEventArgs e)
        {
            if (MyPopup.IsOpen)
            {
                MyPopup.IsOpen = false;
            }
            else
            {
                MyPopup.IsOpen = true;
            }
        }
    }
}
