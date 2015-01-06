using System;
using System.Collections.Generic;
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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public MainViewModel MainViewModel { get; set; }
        public HomePage(MainViewModel mainViewModel)
        {
            InitializeComponent();
            MainViewModel = mainViewModel;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = MainViewModel;
        }

        private void MainTabControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainViewModel.OnClick(sender, e);
        }
    }
}
