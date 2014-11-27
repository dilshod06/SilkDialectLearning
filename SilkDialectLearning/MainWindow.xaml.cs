using MahApps.Metro.Controls;
using SilkDialectLEarningBLL;
using SilkDialectLearningDAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SilkDialectLearning
{
    /// <summary>
    /// A reimplementation of NavigationWindow based on MetroWindow.
    /// </summary>

    public partial class MainWindow : MetroWindow, IUriContext
    {

        private void ToggleFlyout(int index)
        {
            var flyout = this.Flyouts.Items[index] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }
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

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
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

        public void PlaceSceneDots(Grid front, Image SceneImage)
        {
            items.Clear();
            front.Children.Clear();

            double height = SceneImage.ActualHeight;
            double width = SceneImage.ActualWidth;

            foreach (SceneItem item in ViewModel.SceneViewModel.SceneItems)
            {   
                Border dot = new Border();
                TextBlock l = new TextBlock() { Text = dot.Margin.Left.ToString() };
                TextBlock t = new TextBlock() { Text = dot.Margin.Top.ToString() };

                dot.Tag = item;
                dot.Height = (item.Size * height) / 100;
                dot.Width = (item.Size * width) / 100;
                dot.Margin = new Thickness(
                    (item.XPos * width / 100) - dot.Width / 2,
                    (item.YPos * height / 100) - dot.Height / 2,
                    0,
                    0);
                
                
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

        private void MoveTheDots(Grid grd, Image img)
        {
            var width = img.ActualWidth;
            var height = img.ActualHeight;
            foreach (var child in grd.Children)
            {
                var dot = child as Border;
                if (dot == null) continue;
                var point = dot.Tag as SceneItem;
                dot.Height = (point.Size * height) / 100;
                dot.Width = (point.Size * height) / 100;
                if (dot.Height < 10) dot.Height = 10;
                if (dot.Width < 10) dot.Width = 10;
                dot.Margin = new Thickness((point.XPos * width / 100) - dot.Width / 2, (point.YPos * height / 100) - dot.Height / 2, 0, 0);
            }
        }
        
        void dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try 
            {
                Border dot = sender as Border;
                var item = items[dot];
                ViewModel.SceneViewModel.SelectedSceneItem = item;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            //ViewModel.SceneViewModel.SelectedSceneItem = (sender as Border).DataContext as SceneItem;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Navigate(new Navigation.LanguagesPage(this, this.ViewModel));
            PART_Frame.Navigated += PART_Frame_Navigated;
            PART_Frame.Navigating += PART_Frame_Navigating;
            PART_Frame.NavigationFailed += PART_Frame_NavigationFailed;
            PART_Frame.NavigationProgress += PART_Frame_NavigationProgress;
            PART_Frame.NavigationStopped += PART_Frame_NavigationStopped;
            PART_Frame.LoadCompleted += PART_Frame_LoadCompleted;
            PART_Frame.FragmentNavigation += PART_Frame_FragmentNavigation;

            PART_BackButton.Click += PART_BackButton_Click;
            PART_ForwardButton.Click += PART_ForwardButton_Click;
        }

      

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleFlyout(0);
        }

        private void sceneImage_Loaded(object sender, RoutedEventArgs e)
        {
            Image image= sender as Image;
            var mainGrid = image.Parent as Grid;
            //Grid front = new Grid();
            //front.SetBinding(Grid.WidthProperty, new Binding("ActualWidth") { Mode = BindingMode.OneWay });
            //front.SetBinding(Grid.WidthProperty, new Binding("ActualHeight") { Mode = BindingMode.OneWay });
            //front.Background = new SolidColorBrush(Colors.Black);
            //mainGrid.Children.Add(front);
            var grid = mainGrid.FindChild<Grid>("grd");
            grid.Tag = image;
            grid.SizeChanged += grid_SizeChanged;
            PlaceSceneDots(grid, image);
        }

        void grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var grd = sender as Grid;
            MoveTheDots(grd, grd.Tag as Image);
        }

        public ImageSource ConverterImg(byte[] arr)
        {
            byte[] array = arr;
            BitmapImage image = new BitmapImage();
            MemoryStream ms = new MemoryStream(array);
            image.BeginInit();
            image.StreamSource = ms;
            image.EndInit();
            return image as ImageSource;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var a = ScenesTabControl.ItemContainerGenerator.ContainerFromItem((sender as TabControl).SelectedItem);
            //var b = FindControl<Grid>(a, "grd");
            if (e.AddedItems.Count > 0) 
            {
                Scene scene = e.AddedItems[0] as Scene;
                ViewModel.SceneViewModel.SelectedScene = scene;
            }
        }
        private List<Control> AllChildren(DependencyObject parent)
        {
            var _List = new List<Control>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is Control)
                {
                    _List.Add(_Child as Control);
                }
                _List.AddRange(AllChildren(_Child));
            }
            return _List;
        }


        private T FindControl<T>(DependencyObject parentContainer, string controlName)
        {
            var childControls = AllChildren(parentContainer);
            var control = childControls.OfType<Control>().Where(x => x.Name.Equals(controlName)).Cast<T>().First();
            return control;
        }

        public T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            // Check if this object is the specified type
            if (obj is T)
                return obj as T;

            // Check for children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return null;

            // First check all the children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
            }

            // Then check the childrens children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = FindDescendant<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null && child is T)
                    return child as T;
            }

            return null;
        }

        private void grd_Loaded(object sender, RoutedEventArgs e)
        {
            int a;
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            var parent = border.Parent;
            var margin = border.Margin;
            if (border != null)
            {
                items.Add(border, border.DataContext as SceneItem);
            }

        }

        private void ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

    }
}
