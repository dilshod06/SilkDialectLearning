using Microsoft.Win32;
using SilkDialectLearning.DAL;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SilkDialectLearning.Pages
{
    /// <summary>
    /// Interaction logic for EditScenePage.xaml
    /// </summary>
    public partial class EditScenePage : Page
    {
        private Border selectedSceneDot;

        public ObservableCollection<SceneItem> ChangedItems { get; set; }

        public MainViewModel MainViewModel { get; set; }

        private bool isRectDragInProg;

        public EditScenePage(MainViewModel mainViewModel)
        {
            InitializeComponent();
            this.Loaded += EditScenePage_Loaded;
            MainViewModel = mainViewModel;
            ChangedItems = new ObservableCollection<SceneItem>();
        }

        void EditScenePage_Loaded(object sender, RoutedEventArgs e)
        {
            mainGrid.DataContext = MainViewModel.ViewModel.SceneViewModel.SelectedEntity;
        }

        private void dot_Click(object sender, MouseButtonEventArgs e)
        {
            selectedSceneDot = sender as Border;
            if (selectedSceneDot != null)
                selectedSceneDot.BorderThickness = new Thickness(2);
        }

        private void sceneDot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                border.BorderBrush = (Brush)FindResource("ValidationBrush5");
                isRectDragInProg = true;
                border.CaptureMouse();
            }
        }

        private void sceneDot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                border.BorderBrush = (Brush)FindResource("GrayBrush1");
                isRectDragInProg = false;
                border.ReleaseMouseCapture();
            }
        }

        private void sceneDot_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isRectDragInProg) return;

            Border border = sender as Border;
            if (border != null)
            {
                var canvas = VisualTreeHelpers.FindAncestor<Canvas>(border);
                var mousePos = e.GetPosition(canvas);

                double left = mousePos.X - (border.ActualWidth / 2);
                double top = mousePos.Y - (border.ActualHeight / 2);
                border.Margin = new Thickness(left, top, 0, 0);

                var imageWidth = sceneImage.ActualWidth;
                var imageHeight = sceneImage.ActualHeight;
                SceneItem selectedItem = border.DataContext as SceneItem;
                if (selectedItem != null)
                {
                    selectedItem.XPos = ((border.Margin.Left + (border.Width / 2)) * 100) / imageWidth;
                    selectedItem.YPos = ((border.Margin.Top + (border.Height / 2)) * 100) / imageHeight;
                    ChangedItems.Add(selectedItem);
                }
            }
        }

        private async void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedSceneDot == null)
                return;
            Point point = Mouse.GetPosition(bordersGrid);
            byte[] sound = Assistant.SoundToByte(OpenAudio());
            MainViewModel.ViewModel.OnLoading(true, "Please wait saving...");
            await MainViewModel.ViewModel.InsertSceneItem
            (
                sceneImage.ActualWidth,
                sceneImage.ActualHeight,
                point.X,
                point.Y,
                selectedSceneDot.Height,
                selectedSceneDot.CornerRadius.BottomLeft == 0 ? false : true,
                sound
            );
            MainViewModel.ViewModel.OnLoading(false, "");

        }

        private string OpenAudio()
        {
            OpenFileDialog fopen = new OpenFileDialog();
            fopen.Filter = "mp3(*.mp3)|*.mp3";
            fopen.ShowDialog();
            return fopen.FileName;
        }
    }
}
