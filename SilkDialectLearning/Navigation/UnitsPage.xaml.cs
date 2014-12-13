using SilkDialectLearning.Flyouts;
using SilkDialectLearningBLL;
using SilkDialectLearningDAL;
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
