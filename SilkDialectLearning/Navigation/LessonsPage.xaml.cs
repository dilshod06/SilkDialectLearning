using SilkDialectLearning.Flyouts;
using SilkDialectLearningBLL;
using SilkDialectLearningDAL;
using System.Windows.Controls;

namespace SilkDialectLearning.Navigation
{
    /// <summary>
    /// Interaction logic for LessonsPage.xaml
    /// </summary>
    public partial class LessonsPage : Page
    {
        public MainViewModel MainViewModel { get; set; }
        public HomeFlyout HomeFlyout { get; set; }
        public LessonsPage(HomeFlyout HomeFlyout, MainViewModel MainViewModel)
        {
            this.MainViewModel = MainViewModel;
            this.HomeFlyout = HomeFlyout;
            InitializeComponent();
            this.DataContext = this.MainViewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Lesson lesson = e.AddedItems[0] as Lesson;
            MainViewModel.ViewModel.SelectedLesson = lesson;
        }
    }
}
