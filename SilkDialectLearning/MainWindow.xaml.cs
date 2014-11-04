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
            ViewModel.SceneViewModel.HighlightItem += SceneViewModel_HighlightItem;
            ViewModel.SceneViewModel.StopHighlighting += SceneViewModel_StopHighlighting;
        }

        void SceneViewModel_StopHighlighting(object sender, HighlightItemEventArgs e)
        {
            var item = items.FirstOrDefault(i => i.Value == e.HighlightableItem);
            item.Key.Dispatcher.BeginInvoke(new Action(() =>
            {
                item.Key.Background = new SolidColorBrush(Colors.Green);
            }), null);
        }

        void SceneViewModel_HighlightItem(object sender, HighlightItemEventArgs e)
        {
            var item = items.FirstOrDefault(i => i.Value == e.HighlightableItem);
            item.Key.Dispatcher.BeginInvoke(new Action(() =>
            {
                item.Key.Background = new SolidColorBrush(Colors.Red);
            }), null);
        }

        public ViewModel ViewModel { get; private set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlaceSceneDots();
        }

        private void Languages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Language language = e.AddedItems[0] as Language;
            ViewModel.SelectedLanguage = language;
        }

        private void Levels_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Level level = e.AddedItems[0] as Level;
            ViewModel.SelectedLevel = level;
        }

        private void Units_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            Unit unit = e.AddedItems[0] as Unit;
            ViewModel.SelectedUnit = unit;
        }

        private void Lessons_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {
            Lesson lesson = e.AddedItems[0] as Lesson;
            ViewModel.SelectedLesson = lesson;
        }

        private void Scenes_SelectionChanged_4(object sender, SelectionChangedEventArgs e)
        {
            Scene scene = e.AddedItems[0] as Scene;
            ViewModel.SceneViewModel.SelectedScene = scene;
            BitmapSource image = Assistant.ByteToBitmapSource(scene.ScenePicture.Picture);
            img.Source = image;

        }

        private void SceneItems_SelectionChanged_5(object sender, SelectionChangedEventArgs e)
        {
            SceneItem sceneItem = e.AddedItems[0] as SceneItem;
            ViewModel.SceneViewModel.SelectedSceneItem = sceneItem;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton r = sender as RadioButton;
            if (r.Content.ToString() == "Learn")
            {
                ViewModel.SceneViewModel.SceneActivity = Activity.Learn;
            }
            else if (r.Content.ToString() == "Practice")
            {
                ViewModel.SceneViewModel.SceneActivity = Activity.Practice;
            }
        }

        Dictionary<Border, SceneItem> items = new Dictionary<Border, SceneItem>();

        public void PlaceSceneDots()
        {
            items.Clear();
            front.Children.Clear();
            double height = img.ActualHeight;
            double width = img.ActualWidth;

            foreach (SceneItem item in ViewModel.SceneViewModel.SceneItems)
            {
                Border dot = new Border();
                dot.Height = (item.Size * height) / 100;
                dot.Width = (item.Size * height) / 100;
                dot.Margin = new Thickness((item.XPos * width / 100) - dot.Width / 2, (item.YPos * height / 100) - dot.Height / 2, 0, 0);
                dot.Background = new SolidColorBrush(Colors.Green);
                dot.BorderThickness = new Thickness(1, 1, 1, 1);
                dot.BorderBrush = new SolidColorBrush(Colors.Red);
                dot.MouseLeftButtonDown += dot_MouseLeftButtonDown;
                items.Add(dot, item);
                if (item.IsRound)
                {
                    dot.CornerRadius = new CornerRadius(90);
                }
                front.Children.Add(dot);
            }
        }

        void dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border dot = sender as Border;
            var item = items[dot];
            ViewModel.SceneViewModel.SelectedSceneItem = item;
        }
    }
}
