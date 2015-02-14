using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SilkDialectLearning.BLL;
using SilkDialectLearning.Pages;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using SilkDialectLearning.Views;

namespace SilkDialectLearning
{
    /// <summary>
    /// A reimplementation of NavigationWindow based on MetroWindow.
    /// </summary>

    [ContentProperty("OverlayContent")]
// ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow, IUriContext
    {
        private const int LeftMenu = 0;
        private const int HomeMenu = 1;
        private const int SettingsMenu = 2;

        private readonly MainViewModel mainViewModel;
        private bool isFlyoutOpened;

        public MainWindow()
        {
            InitializeComponent();
            this.mainViewModel = new MainViewModel(this);
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.MouseRightButtonDown += MetroWindow_MouseRightButtonDown;
            this.mainViewModel.ViewModel.SceneViewModel.PracticeFinished += SceneViewModel_PracticeFinished;
            this.mainViewModel.ViewModel.LessonSelected += ViewModel_LessonSelected;
        }

        void ViewModel_LessonSelected(object sender, EventArgs e)
        {
            Navigate(new SceneView()); // If some lesson will be selected it will be navigate to SceneView page
        }

        void SceneViewModel_PracticeFinished(object sender, PraceticeFinishedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                var mySettings = new MetroDialogSettings
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No",
                    AnimateHide = false,
                    ColorScheme = mainViewModel.UseAccentForDialogs ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
                };
                MessageDialogResult result = await this.ShowMessageAsync(e.Message, "", MessageDialogStyle.AffirmativeAndNegative, mySettings);
                if (result != MessageDialogResult.Negative)
                {
                    mainViewModel.ViewModel.SceneViewModel.Practice();
                }
            }));
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = mainViewModel;
            this.PART_NavigatePanel.DataContext = this;
            this.mainViewModel.ViewModel.Loading += ViewModel_Loading;
            this.PART_Frame.Navigated += PART_Frame_Navigated;
            this.PART_Frame.Navigating += PART_Frame_Navigating;
            this.PART_Frame.LoadCompleted += PART_Frame_LoadCompleted;
            this.PART_BackButton.Click += PART_BackButton_Click;
            this.Navigate(new HomePage(mainViewModel));
        }


        private void ViewModel_Loading(object sender, LoadingEventArgs e)
        {
            if (e.Loading)
            {
                ShowLoadingMessage(e.Message);
            }
            else
            {
                HideOverlay();
            }
        }

        private void ShowLoadingMessage(string message = "Loading...")
        {
            StackPanel panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical
            };
            ProgressRing progressRing = new ProgressRing
            {
                IsActive = true,
                Width = 40,
                Height = 40
            };
            TextBlock messageTextBlock = new TextBlock
            {
                FontFamily = (FontFamily)FindResource("HeaderFontFamily"),
                FontSize = (double)FindResource("WindowTitleFontSize"),
                Foreground = (Brush)FindResource("WhiteBrush"),
                Text = message,
                Margin = new Thickness(0, 25, 0, 0)
            };
            panel.Children.Add(progressRing);
            panel.Children.Add(messageTextBlock);
            ShowOverlay(panel);
        }

        private void ToggleFlyout(int index)
        {
            isFlyoutOpened = true;
            var flyout = Flyouts.Items[index] as Flyout;
            if (flyout == null)
                return;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(SettingsMenu);
        }

        void PART_Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (LoadCompleted != null)
                LoadCompleted(this, e);
        }

        async void PART_Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Frame frame = sender as Frame;
            if (frame != null)
            {
                var sceneEditPage = frame.Content as EditScenePage;
                if (sceneEditPage != null)
                {//This will be raised after pressing back button and updated Changed SceneItems
                    this.MouseRightButtonDown += MetroWindow_MouseRightButtonDown;
                    if (sceneEditPage.ChangedItems.Count > 0)
                    {
                        mainViewModel.ViewModel.OnLoading(true, "Saving...");
                        await mainViewModel.ViewModel.UpdateAll(sceneEditPage.ChangedItems);
                        mainViewModel.ViewModel.OnLoading(false, "");
                    }
                }
            }
            if (Navigating != null)
                Navigating(this, e);
        }

        void PART_BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanGoBack)
                GoBack();
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            PART_Frame.Navigating -= PART_Frame_Navigating;
            PART_Frame.LoadCompleted -= PART_Frame_LoadCompleted;
            PART_Frame.Navigated -= PART_Frame_Navigated;
            PART_BackButton.Click -= PART_BackButton_Click;
            this.Loaded -= MainWindow_Loaded;
            this.Closing -= MainWindow_Closing;
            this.MouseRightButtonDown -= MetroWindow_MouseRightButtonDown;
            this.mainViewModel.ViewModel.SceneViewModel.PracticeFinished -= SceneViewModel_PracticeFinished;
            this.mainViewModel.ViewModel.LessonSelected -= ViewModel_LessonSelected;
        }

        void PART_Frame_Navigated(object sender, NavigationEventArgs e)
        {
            PART_Title.Content = ((Page)PART_Frame.Content).Title;
            PageContent = PART_Frame.Content;
            if (PageContent is EditScenePage)
            {
                ToggleFlyout(HomeMenu);
                this.MouseRightButtonDown -= MetroWindow_MouseRightButtonDown;
            }
            else if (PageContent is SceneView)
            {
                var entry = PART_Frame.RemoveBackEntry();
                while (entry != null)
                {
                    entry = PART_Frame.RemoveBackEntry();
                }
            }
            PART_BackButton.Visibility = CanGoBack ? Visibility.Visible : Visibility.Hidden;
            PART_NavigatePanel.Visibility = CanGoBack ? Visibility.Visible : Visibility.Collapsed;

            if (Navigated != null)
                Navigated(this, e);
        }

        public static readonly DependencyProperty PageContentProperty = DependencyProperty.Register("PageContent", typeof(object), typeof(MainWindow));

        public object PageContent
        {
            get { return GetValue(PageContentProperty); }
            private set { SetValue(PageContentProperty, value); }
        }

        /// <summary>
        /// Gets an IEnumerable that you use to enumerate the entries in back navigation history for a NavigationWindow.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.ForwardStack"/>
        public IEnumerable ForwardStack { get { return PART_Frame.ForwardStack; } }

        /// <summary>
        /// Gets an IEnumerable that you use to enumerate the entries in back navigation history for a NavigationWindow.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.BackStack"/>
        public IEnumerable BackStack { get { return PART_Frame.BackStack; } }

        /// <summary>
        /// Gets the NavigationService that is used by this MetroNavigationWindow to provide navigation services to its content.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.NavigationService"/>
        public NavigationService NavigationService { get { return PART_Frame.NavigationService; } }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in back navigation history.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.CanGoBack"/>
        public bool CanGoBack { get { return PART_Frame.CanGoBack; } }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in forward navigation history.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.CanGoForward"/>
        public bool CanGoForward { get { return PART_Frame.CanGoForward; } }

        /// <summary>
        /// Gets or sets the base uniform resource identifier (URI) of the current context.
        /// </summary>
        /// <see cref="IUriContext.BaseUri"/>
        Uri IUriContext.BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the uniform resource identifier (URI) of the current content, or the URI of new content that is currently being navigated to.  
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.Source"/>
        public Uri Source { get { return PART_Frame.Source; } set { PART_Frame.Source = value; } }

        /// <summary>
        /// Adds an entry to back navigation history that contains a CustomContentState object.
        /// </summary>
        /// <param name="state">A CustomContentState object that represents application-defined state that is associated with a specific piece of content.</param>
        /// <see cref="System.Windows.Navigation.NavigationWindow.AddBackEntry"/>
        public void AddBackEntry(CustomContentState state)
        {
            PART_Frame.AddBackEntry(state);
        }

        /// <summary>
        /// Removes the most recent journal entry from back history.
        /// </summary>
        /// <returns>The most recent JournalEntry in back navigation history, if there is one.</returns>
        /// <see cref="System.Windows.Navigation.NavigationWindow.RemoveBackEntry"/>
        public JournalEntry RemoveBackEntry()
        {
            return PART_Frame.RemoveBackEntry();
        }

        /// <summary>
        /// Navigates to the most recent item in back navigation history.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.GoBack"/>
        public void GoBack()
        {
            PART_Frame.GoBack();
        }

        /// <summary>
        /// Navigates asynchronously to content that is contained by an object.
        /// </summary>
        /// <param name="content">An Object that contains the content to navigate to.</param>
        /// <returns>true if a navigation is not canceled; otherwise, false.</returns>
        /// <see cref="System.Windows.Navigation.NavigationWindow.Navigate(Object)"/>
        public bool Navigate(Object content)
        {
            return PART_Frame.Navigate(content);
        }

        /// <summary>
        /// Occurs when a new navigation is requested.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.Navigating"/>
        public event NavigatingCancelEventHandler Navigating;

        /// <summary>
        /// Occurs when the content that is being navigated to has been found, and is available from the PageContent property, although it may not have completed loading
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.Navigated"/>
        public event NavigatedEventHandler Navigated;

        /// <summary>
        /// Occurs when content that was navigated to has been loaded, parsed, and has begun rendering.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.LoadCompleted"/>
        public event LoadCompletedEventHandler LoadCompleted;

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            PART_Frame.Navigate(new EditScenePage(mainViewModel));
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(LeftMenu);
            ToggleFlyout(HomeMenu);
        }

        private void MetroWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleFlyout(HomeMenu); // Shows or Hides home menu with mouse right clicking
        }

        private void MainMenu_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(HomeMenu); //Shows or Hides home menu with clicking home button
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isFlyoutOpened)
            {
                ToggleFlyout(HomeMenu);
                MyPopup.IsOpen = false;
                isFlyoutOpened = false;
            }
        }

        private void SceneButton_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new SceneView());
        }
    }
}
