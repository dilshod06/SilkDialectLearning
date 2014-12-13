using SilkDialectLearning.Flyouts;
using SilkDialectLearningBLL;
using SilkDialectLearningDAL;
using System.Windows.Controls;

namespace SilkDialectLearning.Navigation
{
    /// <summary>
    /// Interaction logic for Lessons.xaml
    /// </summary>
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

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var doc = (((sender as Button).Parent as StackPanel).Parent as DockPanel);
            var a = VisualTreeHelpers.FindAncestor<ListBoxItem>(doc);
        }
    }
}
