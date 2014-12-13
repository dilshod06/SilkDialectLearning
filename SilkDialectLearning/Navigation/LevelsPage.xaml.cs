using SilkDialectLearning.Flyouts;
using SilkDialectLearningBLL;
using SilkDialectLearningDAL;
using System.Windows.Controls;

namespace SilkDialectLearning.Navigation
{
    /// <summary>
    /// Interaction logic for Levels.xaml
    /// </summary>
    public partial class LevelsPage : Page
    {
        public MainViewModel MainViewModel { get; set; }
        public HomeFlyout HomeFlyout { get; set; }
        public LevelsPage(HomeFlyout HomeFlyout, MainViewModel MainViewModel)
        {
            this.HomeFlyout = HomeFlyout;
            this.MainViewModel = MainViewModel;
            InitializeComponent();
            this.DataContext = this.MainViewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Level level = e.AddedItems[0] as Level;
                MainViewModel.ViewModel.SelectedLevel = level;
                this.HomeFlyout.Navigate(new UnitsPage(this.HomeFlyout, this.MainViewModel));
            }
        }
    }
}
