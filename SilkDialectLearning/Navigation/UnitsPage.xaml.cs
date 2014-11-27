using SilkDialectLEarningBLL;
using SilkDialectLearningDAL;
using System.Windows.Controls;

namespace SilkDialectLearning.Navigation
{
    /// <summary>
    /// Interaction logic for UnitsPage.xaml
    /// </summary>
    public partial class UnitsPage : Page
    {
        public ViewModel ViewModel { get; set; }
        public MainWindow MainWindow { get; set; }
        public UnitsPage(MainWindow MainWindow, ViewModel ViewModel)
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
                Unit unit = e.AddedItems[0] as Unit;
                ViewModel.SelectedUnit = unit;
                this.MainWindow.Navigate(new LessonsPage(MainWindow, ViewModel));
            }
        }
    }
}
