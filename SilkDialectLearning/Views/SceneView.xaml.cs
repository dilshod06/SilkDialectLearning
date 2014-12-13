using SilkDialectLearningBLL;
using SilkDialectLearningDAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SilkDialectLearning.Views
{
    /// <summary>
    /// Interaction logic for SceneView.xaml
    /// </summary>
    public partial class SceneView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(ViewModel), typeof(SceneView), new PropertyMetadata(null));

        public ViewModel ViewModel
        {
            get { return (ViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private Dictionary<Border, SceneItem> items;
        private Storyboard storyBoard;

        public SceneView()
        {
            InitializeComponent();

        }
        private void SceneViewModel_StopHighlighting(object sender, HighlightItemEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var item = items.FirstOrDefault(i => i.Value == e.HighlightableItem);
                storyBoard.Stop();
                storyBoard.Children.Clear();

            }), null);
        }

        private void SceneViewModel_HighlightItem(object sender, HighlightItemEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var item = items.FirstOrDefault(i => i.Value == e.HighlightableItem);
                ColorAnimation colorAnimation = new ColorAnimation()
                {
                    AutoReverse = true,
                    Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                    RepeatBehavior = RepeatBehavior.Forever,
                    From = (Color)ColorConverter.ConvertFromString("#E1B94C00"),
                    By = (Color)ColorConverter.ConvertFromString("#CCA4C400"),
                    To = (Color)ColorConverter.ConvertFromString("#CCAA00FF")
                };
                PropertyPath colorTargetPath = new PropertyPath("(Border.Background).(SolidColorBrush.Color)");
                storyBoard = new Storyboard();
                Storyboard.SetTarget(colorAnimation, item.Key);
                Storyboard.SetTargetProperty(colorAnimation, colorTargetPath);
                storyBoard.Children.Add(colorAnimation);
                storyBoard.Begin();

            }));
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl.SelectedIndex == -1)
                tabControl.SelectedIndex = 0;

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

        private bool loaded;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            items = new Dictionary<Border, SceneItem>();
            if (!loaded && !DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.ViewModel = (this.DataContext as MainViewModel).ViewModel;
                ViewModel.SceneViewModel.HighlightItem += SceneViewModel_HighlightItem;
                ViewModel.SceneViewModel.StopHighlighting += SceneViewModel_StopHighlighting;
                ViewModel.LessonSelected += ViewModel_LessonSelected;
                loaded = true;
            }
        }

        void ViewModel_LessonSelected(object sender, EventArgs e)
        {
            if (uiPanel.Visibility != System.Windows.Visibility.Visible)
                uiPanel.Visibility = System.Windows.Visibility.Visible;
        }
        private void ScenesTabControl_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as TabControl).SelectedIndex = 0;
        }

    }
}
