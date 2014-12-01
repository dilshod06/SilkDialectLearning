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
        public ViewModel ViewModel { get; set; }
        public MainWindow MainWindow { get; set; }
        public LessonsPage(MainWindow MainWindow, ViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
            this.MainWindow = MainWindow;
            InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Lesson lesson = e.AddedItems[0] as Lesson;
            ViewModel.SelectedLesson = lesson;
        }
    }
}
