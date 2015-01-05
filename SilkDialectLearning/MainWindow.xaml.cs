using MahApps.Metro.Controls;
using SilkDialectLearning.Pages;
using SilkDialectLearningBLL;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;

namespace SilkDialectLearning
{
    /// <summary>
    /// A reimplementation of NavigationWindow based on MetroWindow.
    /// </summary>

    [ContentProperty("OverlayContent")]
    public partial class MainWindow : MetroWindow, IUriContext
    {
        public MainViewModel MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel = new MainViewModel(this);
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = MainWindowViewModel;
            MainWindowViewModel.Loading += ViewModel_Loading;
            PART_NavigatePanel.DataContext = this;
            this.Navigate(new HomePage(MainWindowViewModel));
            PART_Frame.Navigated += PART_Frame_Navigated;
            PART_Frame.Navigating += PART_Frame_Navigating;
            PART_Frame.LoadCompleted += PART_Frame_LoadCompleted;

            PART_BackButton.Click += PART_BackButton_Click;
        }

        void ViewModel_Loading(object sender, LoadingEventArgs e)
        {
            if (e.Loading)
            {
                StackPanel panel = new StackPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Orientation = Orientation.Vertical
                };
                MetroProgressBar progressBar = new MetroProgressBar()
                {
                    IsIndeterminate = true,

                    Minimum = 0,
                    Maximum = 100,
                    Width = 200,
                    Foreground = (Brush)FindResource("AccentColorBrush"),
                    Margin = new Thickness(0, 10, 0, 0)
                };
                ProgressRing progressRing = new ProgressRing()
                {
                    IsActive = true,
                    Width = 50,
                    Height = 50
                };
                TextBlock message = new TextBlock()
                {
                    FontFamily = (FontFamily)FindResource("HeaderFontFamily"),
                    FontSize = (double)FindResource("WindowTitleFontSize"),
                    Foreground = (Brush)FindResource("WhiteBrush"),
                    Text = e.Message,
                    Margin = new Thickness(0, 25, 0, 0)
                };
                panel.Children.Add(progressRing);
                panel.Children.Add(message);
                this.ShowOverlay(panel);
            }
            else
            {
                this.HideOverlay();
            }
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

        private void MetroWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MainWindowViewModel.ViewModel.SelectedLesson != null)
                ToggleFlyout(1);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleFlyout(2);
        }

        private void MenuTextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MetroWindow_MouseRightButtonDown(sender, e);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        void PART_Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (LoadCompleted != null)
                LoadCompleted(this, e);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        void PART_Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (Navigating != null)
                Navigating(this, e);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        void PART_BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanGoBack)
                GoBack();
        }

        [System.Diagnostics.DebuggerNonUserCode]
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PART_Frame.Navigating -= PART_Frame_Navigating;
            PART_Frame.LoadCompleted -= PART_Frame_LoadCompleted;
            PART_Frame.Navigated -= PART_Frame_Navigated;

            PART_BackButton.Click -= PART_BackButton_Click;

            this.Loaded -= MainWindow_Loaded;
            this.Closing -= MainWindow_Closing;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        void PART_Frame_Navigated(object sender, NavigationEventArgs e)
        {
            PART_Title.Content = ((Page)PART_Frame.Content).Title;
            (this as IUriContext).BaseUri = e.Uri;

            PageContent = PART_Frame.Content;

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
        [System.Diagnostics.DebuggerNonUserCode]
        public void AddBackEntry(CustomContentState state)
        {
            PART_Frame.AddBackEntry(state);
        }
        /// <summary>
        /// Removes the most recent journal entry from back history.
        /// </summary>
        /// <returns>The most recent JournalEntry in back navigation history, if there is one.</returns>
        /// <see cref="System.Windows.Navigation.NavigationWindow.RemoveBackEntry"/>
        [System.Diagnostics.DebuggerNonUserCode]
        public JournalEntry RemoveBackEntry()
        {
            return PART_Frame.RemoveBackEntry();
        }

        /// <summary>
        /// Navigates to the most recent item in back navigation history.
        /// </summary>
        /// <see cref="System.Windows.Navigation.NavigationWindow.GoBack"/>
        [System.Diagnostics.DebuggerNonUserCode]
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
        [System.Diagnostics.DebuggerNonUserCode]
        public bool Navigate(Object content)
        {
            return PART_Frame.Navigate(content);
        }
        /// <summary>
        /// Navigates asynchronously to content that is specified by a uniform resource identifier (URI).
        /// </summary>
        /// <param name="source">A Uri object initialized with the URI for the desired content.</param>
        /// <returns>true if a navigation is not canceled; otherwise, false.</returns>
        /// <see cref="System.Windows.Navigation.NavigationWindow.Navigate(Uri)"/>
        [System.Diagnostics.DebuggerNonUserCode]
        public bool Navigate(Uri source)
        {
            return PART_Frame.Navigate(source);
        }
        /// <summary>
        /// Navigates asynchronously to content that is contained by an object, and passes an object that contains data to be used for processing during navigation.
        /// </summary>
        /// <param name="content">An Object that contains the content to navigate to.</param>
        /// <param name="extraData">A Object that contains data to be used for processing during navigation.</param>
        /// <returns>true if a navigation is not canceled; otherwise, false.</returns>
        /// <see cref="System.Windows.Navigation.NavigationWindow.Navigate(Object, Object)"/>
        [System.Diagnostics.DebuggerNonUserCode]
        public bool Navigate(Object content, Object extraData)
        {
            return PART_Frame.Navigate(content, extraData);
        }
        /// <summary>
        /// Navigates asynchronously to source content located at a uniform resource identifier (URI), and pass an object that contains data to be used for processing during navigation.
        /// </summary>
        /// <param name="source">A Uri object initialized with the URI for the desired content.</param>
        /// <param name="extraData">A Object that contains data to be used for processing during navigation.</param>
        /// <returns>true if a navigation is not canceled; otherwise, false.</returns>
        /// <see cref="System.Windows.Navigation.NavigationWindow.Navigate(Uri, Object)"/>
        [System.Diagnostics.DebuggerNonUserCode]
        public bool Navigate(Uri source, Object extraData)
        {
            return PART_Frame.Navigate(source, extraData);
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

    }
}
