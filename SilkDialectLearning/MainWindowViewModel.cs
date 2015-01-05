using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SilkDialectLearning.Models;
using SilkDialectLearningBLL;
using SilkDialectLearningDAL;

namespace SilkDialectLearning
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel(MetroWindow metroWindow)
        {
            // create accent color menu items for the demo
            this.AccentColors = ThemeManager.Accents
                                            .Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                            .ToList();

            // create metro theme color menu items for the demo
            this.AppThemes = ThemeManager.AppThemes
                                           .Select(a => new AppThemeMenuData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                                           .ToList();
            //Select selected default theme
            this.selectedTheme = this.AppThemes
                                      .Where(a => a.Name == ThemeManager.DetectAppStyle(Application.Current).Item1.Name)
                                      .FirstOrDefault();

            //Select selected default accent color
            this.selectedAccent = this.AccentColors
                                      .Where(a => a.Name == ThemeManager.DetectAppStyle(Application.Current).Item2.Name)
                                      .FirstOrDefault();

            InitializeDatabaseFile();
            MetroWinow = metroWindow;
            BrushResources = FindBrushResources();
        }

        #region Methods

        private void InitializeDatabaseFile()
        {
            DirectoryInfo currentDir = new DirectoryInfo(".\\");
            if (currentDir.GetFiles().All(f => f.Name != "SilkDialectLearning.db"))
            {
                MessageBox.Show("Database file not found! We will create one for you");
                Global.CreateDatabase = true;
            }
            Global.DatabasePath = currentDir.FullName + "SilkDialectLearning.db";
            ViewModel = Global.GlobalViewModel;
        }

        protected void DoChangeTheme(AppThemeMenuData selectedTheme)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(selectedTheme.Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        }

        protected void DoChangeAccentColor(AccentColorMenuData selectedAccent)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(selectedAccent.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }

        private IEnumerable<string> FindBrushResources()
        {
            var rd = new ResourceDictionary
            {
                Source = new Uri(@"/MahApps.Metro;component/Styles/Colors.xaml", UriKind.RelativeOrAbsolute)
            };

            var resources = rd.Keys.Cast<object>()
                    .Where(key => rd[key] is Brush)
                    .Select(key => key.ToString())
                    .OrderBy(s => s)
                    .ToList();

            return resources;
        }

        public virtual void OnLoading(bool loading, string message)
        {
            var handler = Loading;
            if (handler != null)
            {
                handler(this, new LoadingEventArgs(loading, message));
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Will be raised when Deleting some Entity
        /// </summary>
        public event LoadingEventHandler Loading;

        public delegate void LoadingEventHandler(object sender, LoadingEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// This property gets and sets name of Entities
        /// </summary>
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// This property gets and sets Description Of Entity
        /// </summary>
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Gets and sets instance of ViewModel
        /// </summary>
        public ViewModel ViewModel { get; private set; }

        /// <summary>
        /// gets and sets instance of MainWindow
        /// </summary>
        public MetroWindow MetroWinow { get; private set; }

        /// <summary>
        /// Gets and sets Selected Theme
        /// </summary>
        private AppThemeMenuData selectedTheme;
        public AppThemeMenuData SelectedTheme
        {
            get { return selectedTheme; }
            set
            {
                selectedTheme = value;
                DoChangeTheme(selectedTheme);
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets and sets selected accent color
        /// </summary>
        private AccentColorMenuData selectedAccent;
        public AccentColorMenuData SelectedAccent
        {
            get { return selectedAccent; }
            set
            {
                selectedAccent = value;
                DoChangeAccentColor(selectedAccent);
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets and sets list of Accent colors
        /// </summary>
        public List<AccentColorMenuData> AccentColors { get; set; }
        public List<AppThemeMenuData> AppThemes { get; set; }

        /// <summary>
        /// Gets and sets UseAccentForDialogs checkbox 
        /// </summary>
        private bool useAccentForDialogs;
        public bool UseAccentForDialogs
        {
            get { return useAccentForDialogs; }
            set { useAccentForDialogs = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Gets and sets Brush Resources
        /// </summary>
        public IEnumerable<string> BrushResources { get; private set; }

        #endregion

        #region Commands

        /// <summary>
        /// This property deletes items from HomePage's ListBox
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                return new DeleteCmd(this.MetroWinow, this);
            }
        }

        /// <summary>
        /// This class to used for Deleting Entities
        /// </summary>
        public class DeleteCmd : ICommand
        {
            private MetroWindow metroWindow { get; set; }
            private MainViewModel mainViewModel { get; set; }

            public DeleteCmd(MetroWindow metroWindow, MainViewModel mainViewModel)
            {
                this.metroWindow = metroWindow;
                this.mainViewModel = mainViewModel;
                CanExecuteChanged += DeleteCmd_CanExecuteChanged;
            }

            private void DeleteCmd_CanExecuteChanged(object sender, EventArgs e)
            {
                // TODO: Some logic for CanExecute
            }

            public void Execute(object parameter)
            {
                ListBoxItem listBoxItem = parameter as ListBoxItem;
                if (listBoxItem == null)
                    return;
                ListBox listBox = VisualTreeHelpers.FindAncestor<ListBox>(listBoxItem);
                if (listBox == null)
                    return;
                DeleteEntity(listBoxItem, listBox);
            }

            public async void DeleteEntity(ListBoxItem listBoxItem, ListBox listBox)
            {
                IEntity selectedEntity = listBoxItem.DataContext as IEntity;
                ViewModel viewModel = mainViewModel.ViewModel;

                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No",
                    AnimateHide = false,
                    ColorScheme = mainViewModel.UseAccentForDialogs ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
                };

                if (selectedEntity is Language)
                {
                    MessageDialogResult result = await metroWindow.ShowMessageAsync
                        (
                            string.Format("Do you want to delete this Languge \"{0}\"?", selectedEntity.Name),
                            "It will be permanently deleted.",
                            MessageDialogStyle.AffirmativeAndNegative,
                            mySettings
                        );
                    if (result != MessageDialogResult.Negative)
                    {
                        mainViewModel.OnLoading(true, "Please wait language is deleting...");
                        await viewModel.DeleteLanguage(selectedEntity as Language);
                        viewModel.NotifyPropertyChanged("Languages");
                        mainViewModel.OnLoading(false, "");
                    }
                }
                else if (selectedEntity is Level)
                {
                    MessageDialogResult result = await metroWindow.ShowMessageAsync
                        (
                            string.Format("Do you want to delete this Level \"{0}\" ?", selectedEntity.Name),
                            "It will be permanently deleted.",
                            MessageDialogStyle.AffirmativeAndNegative,
                            mySettings
                        );
                    if (result != MessageDialogResult.Negative)
                    {
                        mainViewModel.OnLoading(true, "Please wait level is deleting...");
                        await viewModel.DeleteLevel(selectedEntity as Level);
                        viewModel.NotifyPropertyChanged("Levels");
                        mainViewModel.OnLoading(false, "");
                    }
                }

                else if (selectedEntity is Unit)
                {
                    MessageDialogResult result = await metroWindow.ShowMessageAsync
                        (
                            string.Format("Do you want to delete this Unit \"{0}\" ?", selectedEntity.Name),
                            "It will be permanently deleted.",
                            MessageDialogStyle.AffirmativeAndNegative,
                            mySettings
                        );
                    if (result != MessageDialogResult.Negative)
                    {
                        mainViewModel.OnLoading(true, "Please wait unit is deleting...");
                        await viewModel.DeleteUnit(selectedEntity as Unit);
                        viewModel.NotifyPropertyChanged("Units");
                        mainViewModel.OnLoading(false, "");
                    }
                }
                else if (selectedEntity is Lesson)
                {
                    MessageDialogResult result =
                        await metroWindow.ShowMessageAsync
                        (
                            string.Format("Do you want to delete this Lesson \"{0}\" ?", selectedEntity.Name),
                            "It will be permanently deleted.",
                            MessageDialogStyle.AffirmativeAndNegative,
                            mySettings
                       );
                    if (result != MessageDialogResult.Negative)
                    {
                        mainViewModel.OnLoading(true, "Please wait lesson is deleting...");
                        await viewModel.DeleteLesson(selectedEntity as Lesson);
                        viewModel.NotifyPropertyChanged("Lessons");
                        mainViewModel.OnLoading(false, "");
                    }
                }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }

        /// <summary>
        /// This command property sets new Entity
        /// </summary>
        public ICommand AddCommand
        {
            get
            {
                return new AddCmd(this.MetroWinow, this);
            }
        }

        /// <summary>
        /// This class to used for adding new Entities 
        /// </summary>
        public class AddCmd : ICommand
        {
            /// <summary>
            /// Gets and Sets MetroWindow instance
            /// </summary>
            private MetroWindow metroWindow { get; set; }

            /// <summary>
            /// Gets and Sets instance of MainViewModel
            /// </summary>
            private MainViewModel mainViewModel { get; set; }

            public AddCmd(MetroWindow metroWindow, MainViewModel mainViewModel)
            {
                this.metroWindow = metroWindow;
                this.mainViewModel = mainViewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                AddEntity(parameter as string);
            }

            public async void AddEntity(string title)
            {
                ViewModel viewModel = mainViewModel.ViewModel;
                var mySettings = new MetroDialogSettings
                {
                    AnimateHide = false,
                    ColorScheme = mainViewModel.UseAccentForDialogs ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
                };
                if (title.Equals("Languages"))
                {
                    if (string.IsNullOrEmpty(mainViewModel.Name))
                        return;
                    mainViewModel.OnLoading(true, "");
                    await viewModel.InsertEntity(new Language
                    {
                        Id = Guid.NewGuid(),
                        Name = mainViewModel.Name,
                        Description = mainViewModel.Description
                    });
                    mainViewModel.OnLoading(false, "");
                    viewModel.NotifyPropertyChanged("Languages");
                    mainViewModel.Name = null;
                    mainViewModel.Description = null;
                }
                else if (title.Equals("Levels"))
                {
                    if (string.IsNullOrEmpty(mainViewModel.Name))
                        return;

                    await viewModel.InsertEntity(new Level
                    {
                        Id = Guid.NewGuid(),
                        Name = mainViewModel.Name,
                        Description = mainViewModel.Description,
                        Language = mainViewModel.ViewModel.SelectedLanguage
                    });
                    viewModel.NotifyPropertyChanged("Levels");
                    mainViewModel.Name = null;
                    mainViewModel.Description = null;
                }
                else if (title.Equals("Units"))
                {
                    if (string.IsNullOrEmpty(mainViewModel.Name))
                        return;

                    await viewModel.InsertEntity(new Unit
                    {
                        Id = Guid.NewGuid(),
                        Name = mainViewModel.Name,
                        Description = mainViewModel.Description,
                        Level = mainViewModel.ViewModel.SelectedLevel
                    });
                    viewModel.NotifyPropertyChanged("Units");
                    mainViewModel.Name = null;
                    mainViewModel.Description = null;
                }
                else if (title.Equals("Lessons"))
                {
                    if (string.IsNullOrEmpty(mainViewModel.Name))
                        return;
                    await viewModel.InsertEntity(new Lesson
                    {
                        Id = Guid.NewGuid(),
                        Name = mainViewModel.Name,
                        Description = mainViewModel.Description,
                        Unit = mainViewModel.ViewModel.SelectedUnit
                    });
                    viewModel.NotifyPropertyChanged("Lessons");
                    mainViewModel.Name = null;
                    mainViewModel.Description = null;
                }
            }
        }

        public ICommand EditCommand { get { return new EditCmd(); } }

        public class EditCmd : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                ListBoxItem listBoxItem = parameter as ListBoxItem;
                if (listBoxItem == null)
                    return;
                ListBox listBox = VisualTreeHelpers.FindAncestor<ListBox>(listBoxItem);
                if (listBox == null)
                    return;
                EditEntity(metroWindow, listBoxItem, listBox);
            }

            public async void EditEntity(MetroWindow metroWindow, ListBoxItem listBoxItem, ListBox listBox)
            {
                // Gets ViewModel from MainViewModel
                MainViewModel mainViewModel = listBox.DataContext as MainViewModel;

                if (mainViewModel == null)
                    return;

                IEntity selectedEntity = listBoxItem.DataContext as IEntity;
                ViewModel viewModel = mainViewModel.ViewModel;

                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No",
                    AnimateHide = false,
                    ColorScheme = mainViewModel.UseAccentForDialogs ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
                };
                if (selectedEntity is Language)
                {
                    string name = await metroWindow.ShowInputAsync("Edit Language", "Enter the name of Language.", mySettings);
                    if (string.IsNullOrEmpty(name)) return;
                    string description = await metroWindow.ShowInputAsync("Edit Language", "Enter the description of Language.", mySettings);
                    if (description == null) return;
                    selectedEntity.Name = name;
                    selectedEntity.Description = description;
                    ModelManager.Db.Update(selectedEntity, typeof(Language));
                }
                else if (selectedEntity is Level)
                {
                    string name = await metroWindow.ShowInputAsync("Edit Level", "Enter the name of Level.", mySettings);
                    if (string.IsNullOrEmpty(name)) return;
                    string description = await metroWindow.ShowInputAsync("Edit Level", "Enter the description of Level.", mySettings);
                    if (description == null) return;
                    selectedEntity.Name = name;
                    selectedEntity.Description = description;
                    ModelManager.Db.Update(selectedEntity, typeof(Level));
                }
            }
        }

        #endregion

        #region Notify

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    public class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }
    }
    public class AppThemeMenuData : AccentColorMenuData { }
}
