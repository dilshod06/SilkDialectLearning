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
	public class AccentColorMenuData
	{
		public string Name { get; set; }
		public Brush BorderColorBrush { get; set; }
		public Brush ColorBrush { get; set; }

		private ICommand changeAccentCommand;

		public ICommand ChangeAccentCommand
		{
			get { return this.changeAccentCommand ?? (changeAccentCommand = new SimpleCommand { CanExecuteDelegate = x => true, ExecuteDelegate = x => this.DoChangeTheme(x) }); }
		}

		protected virtual void DoChangeTheme(object sender)
		{
			var theme = ThemeManager.DetectAppStyle(Application.Current);
			var accent = ThemeManager.GetAccent(this.Name);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
		}
	}

	public class AppThemeMenuData : AccentColorMenuData
	{
		protected override void DoChangeTheme(object sender)
		{
			var theme = ThemeManager.DetectAppStyle(Application.Current);
			var appTheme = ThemeManager.GetAppTheme(this.Name);
			ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
		}
	}

	public class MainViewModel : INotifyPropertyChanged
	{
		public ViewModel ViewModel { get; private set; }

		public MainViewModel()
		{
			// create accent color menu items for the demo
			this.AccentColors = ThemeManager.Accents
											.Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
											.ToList();

			// create metro theme color menu items for the demo
			this.AppThemes = ThemeManager.AppThemes
										   .Select(a => new AppThemeMenuData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
										   .ToList();

			InitializeDatabaseFile();

			BrushResources = FindBrushResources();

		}
		
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

		public List<AccentColorMenuData> AccentColors { get; set; }
		
        public List<AppThemeMenuData> AppThemes { get; set; }

		private bool useAccentForDialogs;
		
        public bool UseAccentForDialogs
		{
			get { return useAccentForDialogs; }
			set { useAccentForDialogs = value; NotifyPropertyChanged(); }
		}

		/// <summary>
		/// This property deletes items from HomePage's ListBox
		/// </summary>
		public ICommand DeleteCommand { get { return new DeleteCmd(); } }

		/// <summary>
		/// This class to used for Deleting Entities
		/// </summary>
		public class DeleteCmd : ICommand
		{
			public void Execute(object parameter)
			{
				var metroWindow = (Application.Current.MainWindow as MetroWindow);
				ListBoxItem listBoxItem = parameter as ListBoxItem;
				if (listBoxItem == null)
					return;
				ListBox listBox = VisualTreeHelpers.FindAncestor<ListBox>(listBoxItem);
				if (listBox == null)
					return;
				DeleteEntity(metroWindow, listBoxItem, listBox);

			}
			public async void DeleteEntity(MetroWindow metroWindow, ListBoxItem listBoxItem, ListBox listBox)
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
					MessageDialogResult result =
						await metroWindow.ShowMessageAsync
						(
							string.Format("Do you want to delete this Languge \"{0}\"?", selectedEntity.Name),
							"It will be permanently deleted.",
							MessageDialogStyle.AffirmativeAndNegative,
							mySettings
						);
					if (result != MessageDialogResult.Negative)
						viewModel.Languages.Remove(selectedEntity as Language);
				}
				else if (selectedEntity is Level)
				{
					MessageDialogResult result =
						await metroWindow.ShowMessageAsync
						(
							string.Format("Do you want to delete this Level \"{0}\" ?", selectedEntity.Name),
							"It will be permanently deleted.",
							MessageDialogStyle.AffirmativeAndNegative,
							mySettings
						);
					if (result != MessageDialogResult.Negative)
						viewModel.Levels.Remove(selectedEntity as Level);
				}

				else if (selectedEntity is Unit)
				{
					MessageDialogResult result =
						await metroWindow.ShowMessageAsync
						(
							string.Format("Do you want to delete this Unit \"{0}\" ?", selectedEntity.Name),
							"It will be permanently deleted.",
							MessageDialogStyle.AffirmativeAndNegative,
							mySettings
						);
					if (result != MessageDialogResult.Negative)
						viewModel.Units.Remove(selectedEntity as Unit);
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
						viewModel.Lessons.Remove(selectedEntity as Lesson);
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
		public ICommand AddCommand { get { return new AddCmd(); } }

		/// <summary>
		/// This class to used for adding new Entities 
		/// </summary>
		public class AddCmd : ICommand
		{
			public bool CanExecute(object parameter)
			{
				return true;
			}

			public event EventHandler CanExecuteChanged;

			public void Execute(object parameter)
			{
				var metroWindow = (Application.Current.MainWindow as MetroWindow);
				string title = parameter as string;
				if (metroWindow == null) return;
				AddEntity(metroWindow, title);
			}

			public async void AddEntity(MetroWindow metroWindow, string title)
			{
				MainViewModel mainViewModel = metroWindow.DataContext as MainViewModel;
				if (mainViewModel == null)
					return;

				var mySettings = new MetroDialogSettings()
				{
					AnimateHide = false,
					ColorScheme = mainViewModel.UseAccentForDialogs ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
				};
				if (title.Equals("Languages"))
				{
					string name = await metroWindow.ShowInputAsync("Add new Language", "Enter the name of Language.", mySettings);
					if (!string.IsNullOrEmpty(name))
					{
						string description =
							await metroWindow.ShowInputAsync("Add new Language", "Enter the description of Language.", mySettings);
						if (description == null)
							return;
						mainViewModel.ViewModel.Languages.Add(
							new Language
							{
								Id = Guid.NewGuid(),
								Name = name,
								Description = description
							});
					}
				}
				else if (title.Equals("Levels"))
				{
					string name = await metroWindow.ShowInputAsync("Add new Level", "Enter the name of Level.", mySettings);
					if (!string.IsNullOrEmpty(name))
					{
						string description =
							await metroWindow.ShowInputAsync("Add new Level", "Enter the description of Level.", mySettings);
						if (description == null)
							return;
						mainViewModel.ViewModel.Levels.Add(
							new Level
							{
								Id = Guid.NewGuid(),
								Name = name,
								Description = description,
								Language = mainViewModel.ViewModel.SelectedLanguage
							});
					}
				}
				else if (title.Equals("Units"))
				{
					string name = await metroWindow.ShowInputAsync("Add new Unit", "Enter the name of Unit.", mySettings);
					if (!string.IsNullOrEmpty(name))
					{
						string description =
							await metroWindow.ShowInputAsync("Add new Unit", "Enter the description of Unit.", mySettings);
						mainViewModel.ViewModel.Units.Add(
							new Unit
							{
								Id = Guid.NewGuid(),
								Name = name,
								Description = description,
								Level = mainViewModel.ViewModel.SelectedLevel
							});
					}
				}
				else if (title.Equals("Lessons"))
				{
					string name = await metroWindow.ShowInputAsync("Add new Lesson", "Enter the name of Lesson.");
					if (!string.IsNullOrEmpty(name))
					{
						string description = await metroWindow.ShowInputAsync("Add new Lesson", "Enter the description of Lesson.");
						if (description == null)
							return;
						mainViewModel.ViewModel.Lessons.Add(
							new Lesson
							{
								Id = Guid.NewGuid(),
								Name = name,
								Description = description,
								Unit = mainViewModel.ViewModel.SelectedUnit
							});
					}
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
		
		public IEnumerable<string> BrushResources { get; private set; }

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
}
