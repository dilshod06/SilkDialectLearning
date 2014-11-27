using SilkDialectLEarningBLL;
using SilkDialectLearningDAL;
using System.Windows.Controls;

namespace SilkDialectLearning.Navigation
{
    /// <summary>
    /// Interaction logic for Levels.xaml
    /// </summary>
    public partial class LevelsPage : Page
    {
        public ViewModel ViewModel { get; set; }
        public MainWindow MainWindow { get; set; }
        public LevelsPage(MainWindow MainWindow, ViewModel ViewModel)
        {
            this.MainWindow = MainWindow;
            this.ViewModel = ViewModel;
            InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Level level = e.AddedItems[0] as Level;
                ViewModel.SelectedLevel = level;
                this.MainWindow.Navigate(new UnitsPage(this.MainWindow, this.ViewModel));
            }
        }
    }
}
