using SilkDialectLEarningBLL;
using SilkDialectLearningDAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SilkDialectLearning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DirectoryInfo currentDir = new DirectoryInfo(".\\");
            if (!currentDir.GetFiles().Any(f => f.Name == "SilkDialectLearning.3db"))
            {
                MessageBox.Show("Database file not found! We will create one for you");
                Global.CreateDatabase = true;
            }
            Global.DatabasePath = currentDir.FullName + "SilkDialectLearning.3db";
            ViewModel = Global.GlobalViewModel;
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        public ViewModel ViewModel { get; private set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var languages = ViewModel.Languages;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Language language = e.AddedItems[0] as Language;
            ViewModel.SelectedLanguage = language;
        }

        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Level level = e.AddedItems[0] as Level;
            ViewModel.SelectedLevel = level;
        }

        private void ListBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            Unit unit = e.AddedItems[0] as Unit;
            ViewModel.SelectedUnit = unit;
        }

        private void ListBox_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {
            Lesson lesson = e.AddedItems[0] as Lesson;
            ViewModel.SelectedLesson = lesson;
        }
    }
}
