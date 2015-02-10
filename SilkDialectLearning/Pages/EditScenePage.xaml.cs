using SilkDialectLearning.DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SilkDialectLearning.Pages
{
    /// <summary>
    /// Interaction logic for EditScenePage.xaml
    /// </summary>
    public partial class EditScenePage : Page
    {
        public ObservableCollection<SceneItem> ChangedItems { get; set; }

        public MainViewModel MainViewModel { get; set; }
        
        private bool isRectDragInProg = false;

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


        private void Dots_Click(object sender, MouseButtonEventArgs e)
        {

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
                selectedItem.XPos = ((border.Margin.Left + (border.Width / 2)) * 100) / imageWidth;
                selectedItem.YPos = ((border.Margin.Top + (border.Height / 2)) * 100) / imageHeight;
                ChangedItems.Add(selectedItem);
            }
        }
    }
}
