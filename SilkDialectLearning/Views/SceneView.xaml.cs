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
        public ViewModel ViewModel
        {
            get { return (ViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ViewModel), typeof(SceneView));


        private Dictionary<Border, SceneItem> items;
        private Storyboard storyBoard;
        //public ViewModel ViewModel { get; set; }
        public SceneView()
        {
            InitializeComponent();
            
        }
        void SceneViewModel_StopHighlighting(object sender, HighlightItemEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var item = items.FirstOrDefault(i => i.Value == e.HighlightableItem);
                storyBoard.Stop();
                storyBoard.Children.Clear();

            }), null);
        }

        void SceneViewModel_HighlightItem(object sender, HighlightItemEventArgs e)
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
            var scene = (sender as TabControl).SelectedItem as Scene;
            if (scene != null)
            {
                ViewModel.SceneViewModel.SelectedScene = scene;
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
        void dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        bool loaded;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded && !DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.ViewModel = this.DataContext as ViewModel;
                ViewModel.SceneViewModel.HighlightItem += SceneViewModel_HighlightItem;
                ViewModel.SceneViewModel.StopHighlighting += SceneViewModel_StopHighlighting;
                items = new Dictionary<Border, SceneItem>();
                loaded = true;
            }

        }
    }
}
