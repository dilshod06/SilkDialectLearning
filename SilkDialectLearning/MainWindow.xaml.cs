using MahApps.Metro.Controls;
using SilkDialectLearningBLL;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace SilkDialectLearning
{
    /// <summary>
    /// A reimplementation of NavigationWindow based on MetroWindow.
    /// </summary>

    public partial class MainWindow : MetroWindow, IUriContext
    {
        
        public ViewModel ViewModel { get; private set; }

        public MainWindow()
        {
            InitializeDatabaseFile();
            InitializeComponent();
            this.DataContext = ViewModel;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }
       
        private void InitializeDatabaseFile()
        {
            DirectoryInfo currentDir = new DirectoryInfo(".\\");
            if (!currentDir.GetFiles().Any(f => f.Name == "SilkDialectLearning.db"))
            {
                MessageBox.Show("Database file not found! We will create one for you");
                Global.CreateDatabase = true;
            }
            Global.DatabasePath = currentDir.FullName + "SilkDialectLearning.db";
            ViewModel = Global.GlobalViewModel;
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
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

        [System.Diagnostics.DebuggerNonUserCode]
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PART_Frame.FragmentNavigation -= PART_Frame_FragmentNavigation;
            PART_Frame.Navigating -= PART_Frame_Navigating;
            PART_Frame.NavigationFailed -= PART_Frame_NavigationFailed;
            PART_Frame.NavigationProgress -= PART_Frame_NavigationProgress;
            PART_Frame.NavigationStopped -= PART_Frame_NavigationStopped;
            PART_Frame.LoadCompleted -= PART_Frame_LoadCompleted;
            PART_Frame.Navigated -= PART_Frame_Navigated;

            PART_ForwardButton.Click -= PART_ForwardButton_Click;
            PART_BackButton.Click -= PART_BackButton_Click;

            this.Loaded -= MainWindow_Loaded;
            this.Closing -= MainWindow_Closing;
        }
        
        private void ToggleFlyout(int index)
        {
            var flyout = this.Flyouts.Items[index] as Flyout;
            if (flyout == null)
                return;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleFlyout(0);
        }
    }
}
