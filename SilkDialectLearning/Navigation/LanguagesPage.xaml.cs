using SilkDialectLEarningBLL;
using SilkDialectLearningDAL;
using System.Windows.Controls;

namespace SilkDialectLearning.Navigation
{
    /// <summary>
    /// Interaction logic for Lessons.xaml
    /// </summary>
    public partial class LanguagesPage : Page
    {
        public ViewModel ViewModel { get; set; }
        public MainWindow MainWindow { get; set; }
        public LanguagesPage(MainWindow MainWindow, ViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
            this.MainWindow = MainWindow;
            InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        private void Languages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Language language = e.AddedItems[0] as Language;
                ViewModel.SelectedLanguage = language;
                this.MainWindow.Navigate(new LevelsPage(this.MainWindow, this.ViewModel));
            }
        }
    }
}
