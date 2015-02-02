using SilkDialectLearningBLL;
using SilkDialectLearningDAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SilkDialectLearning.Views
{
    /// <summary>
    /// Interaction logic for SceneView.xaml
    /// </summary>
    public partial class SceneView
    {
        private int lastSelectedIndex;

        private bool loaded;

        private Dictionary<Border, SceneItem> items;

        private Storyboard storyBoard;

        public ViewModel ViewModel { get; set; }

        public SceneView()
        {
            InitializeComponent();
        }

        private void ScenesTabControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl != null) tabControl.SelectedIndex = lastSelectedIndex;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl != null)
            {
                lastSelectedIndex = tabControl.SelectedIndex;
                if (tabControl.SelectedIndex == -1)
                {
                    lastSelectedIndex = 0;
                    tabControl.SelectedIndex = 0;
                }
            }
            items = new Dictionary<Border, SceneItem>();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                items.Add(border, border.DataContext as SceneItem);
            }
        }

        private void dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Border border = sender as Border;
                if (border != null)
                    ViewModel.SceneViewModel.SelectedSceneItem = border.DataContext as SceneItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            items = new Dictionary<Border, SceneItem>();
            if (!loaded && !DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                MainViewModel mainViewModel = DataContext as MainViewModel;
                if (mainViewModel != null)
                    ViewModel = mainViewModel.ViewModel;
                ViewModel.SceneViewModel.HighlightItem += SceneViewModel_HighlightItem;
                ViewModel.SceneViewModel.StopHighlighting += SceneViewModel_StopHighlighting;
                loaded = true;
            }
        }

        private void SceneViewModel_StopHighlighting(object sender, HighlightItemEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var item = items.FirstOrDefault(i => i.Value == e.HighlightableItem);
                if (item.Key != null)
                    item.Key.Opacity = .5;
                storyBoard.Stop();
                storyBoard.Children.Clear();

            }), null);
        }

        private void SceneViewModel_HighlightItem(object sender, HighlightItemEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Color? colorFrom = null;
                Color? colorTo = null;
                if (e.PracticeItemResult == PracticeItemResult.Default)
                {
                    colorFrom = (Color)ColorConverter.ConvertFromString("#CC2700B9");
                    colorTo = (Color)ColorConverter.ConvertFromString("#F665A505");
                    
                }
                else if (e.PracticeItemResult == PracticeItemResult.Wrong)
                {
                    colorFrom = (Color)ColorConverter.ConvertFromString("#FCB90C00");
                    colorTo = (Color)ColorConverter.ConvertFromString("#F6FF0051");
                }
                else if (e.PracticeItemResult == PracticeItemResult.Fixed)
                {
                    colorFrom = (Color)ColorConverter.ConvertFromString("#FCECE312");
                    colorTo = (Color)ColorConverter.ConvertFromString("#DEDBDE26");
                }
                else if (e.PracticeItemResult == PracticeItemResult.Right)
                {
                    colorFrom = (Color)ColorConverter.ConvertFromString("#B92BB313");
                    colorTo = (Color)ColorConverter.ConvertFromString("#FC0A770F");
                }

                var item = items.FirstOrDefault(i => i.Value == e.HighlightableItem);
                item.Key.Opacity = 1;
                ColorAnimation colorAnimation = new ColorAnimation
                {
                    AutoReverse = true,
                    Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                    RepeatBehavior = RepeatBehavior.Forever,
                    From = colorFrom,
                    To = colorTo
                };
                PropertyPath colorTargetPath = new PropertyPath("(Border.Background).(SolidColorBrush.Color)");
                storyBoard = new Storyboard();
                Storyboard.SetTarget(colorAnimation, item.Key);
                Storyboard.SetTargetProperty(colorAnimation, colorTargetPath);
                storyBoard.Children.Add(colorAnimation);
                storyBoard.Begin();
            }));
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as Button).DataContext = this.DataContext;
        }

    }
}
