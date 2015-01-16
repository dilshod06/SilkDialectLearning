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
    public partial class SceneView : UserControl
    {
        private int lastSelectedIndex = 0;

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
            (sender as TabControl).SelectedIndex = lastSelectedIndex;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            lastSelectedIndex = tabControl.SelectedIndex;
            if (tabControl.SelectedIndex == -1)
            {
                lastSelectedIndex = 0;
                tabControl.SelectedIndex = 0;
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
                ViewModel.SceneViewModel.SelectedSceneItem = (sender as Border).DataContext as SceneItem;
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
                ViewModel = (this.DataContext as MainViewModel).ViewModel;
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
                    (item.Key as Border).Opacity = .5;
                storyBoard.Stop();
                storyBoard.Children.Clear();

            }), null);
        }

        private void SceneViewModel_HighlightItem(object sender, HighlightItemEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var item = items.FirstOrDefault(i => i.Value == e.HighlightableItem);
                (item.Key as Border).Opacity = 1;
                ColorAnimation colorAnimation = new ColorAnimation()
                {
                    AutoReverse = true,
                    Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                    RepeatBehavior = RepeatBehavior.Forever,
                    From = (Color)ColorConverter.ConvertFromString("#CC2700B9"),
                    To = (Color)ColorConverter.ConvertFromString("#DCFF0010")
                };
                PropertyPath colorTargetPath = new PropertyPath("(Border.Background).(SolidColorBrush.Color)");
                storyBoard = new Storyboard();
                Storyboard.SetTarget(colorAnimation, item.Key);
                Storyboard.SetTargetProperty(colorAnimation, colorTargetPath);
                storyBoard.Children.Add(colorAnimation);
                storyBoard.Begin();
            }));
        }

    }
}
