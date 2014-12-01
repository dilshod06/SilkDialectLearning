using SilkDialectLearningDAL;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SilkDialectLearningBLL
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {

        }
        public ViewModel(string dbPath, string userName = "", string password = "", bool createDatabase = false)
        {
            var db = new Entities(dbPath, createDatabase);
            Db = db;
            languages = new ObservableCollection<Language>(db.GetEntities<Language>());
        }

        /// <summary>
        /// Will be raised when a language gets selected
        /// </summary>
        public event EventHandler LanguageSelected;

        /// <summary>
        /// Will be raised when a level gets selected
        /// </summary>
        public event EventHandler LevelSelected;

        /// <summary>
        /// Will be raised when a unit gets selected
        /// </summary>
        public event EventHandler UnitSelected;

        /// <summary>
        /// Will be raised when a lesson gets selected
        /// </summary>
        public event EventHandler LessonSelected;

        /// <summary>
        /// Access to the database
        /// </summary>
        internal Entities Db { get; private set; }

        User user;
        /// <summary>
        /// Returns the current logged in user
        /// </summary>
        public User User { get; private set; }

        ObservableCollection<Language> languages;
        /// <summary>
        /// Returns existing languages in the database 
        /// </summary>
        public ObservableCollection<Language> Languages { get { return languages; } }

        Language language;
        /// <summary>
        /// Returns the last selected language
        /// </summary>
        public Language SelectedLanguage
        {
            get
            {
                return language;
            }
            set
            {
                language = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("Levels");
                var languageSelected = LanguageSelected;
                if (languageSelected != null)
                {
                    languageSelected(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Returns the list of levels under selected language
        /// </summary>
        public ObservableCollection<Level> Levels
        {
            get
            {
                ObservableCollection<Level> levels = new ObservableCollection<Level>();
                if (SelectedLanguage != null)
                {
                    levels = SelectedLanguage.Levels;
                }
                return levels;
            }
        }

        Level selectedLevel;
        /// <summary>
        /// Returns the last selected level
        /// </summary>
        public Level SelectedLevel
        {
            get
            {
                return selectedLevel;
            }
            set
            {
                selectedLevel = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("Units");
                var levelSelected = LevelSelected;
                if (levelSelected != null)
                {
                    levelSelected(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Returns the list of units under selected level
        /// </summary>
        public ObservableCollection<Unit> Units
        {
            get
            {
                ObservableCollection<Unit> units = new ObservableCollection<Unit>();
                if (SelectedLevel != null)
                {
                    units = SelectedLevel.Units;
                }
                return units;
            }
        }

        private Unit selectedUnit;
        /// <summary>
        /// Returns the last selected unit
        /// </summary>
        public Unit SelectedUnit
        {
            get
            {
                return selectedUnit;
            }
            set
            {
                selectedUnit = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("Lessons");
                var unitSelected = UnitSelected;
                if (unitSelected != null)
                {
                    unitSelected(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Returns the list of lessons under selected unit
        /// </summary>
        public ObservableCollection<Lesson> Lessons
        {
            get
            {
                ObservableCollection<Lesson> lessons = new ObservableCollection<Lesson>();
                if (SelectedUnit != null)
                {
                    lessons = SelectedUnit.Lessons;
                }
                return lessons;
            }
        }

        private Lesson selectedLesson;
        /// <summary>
        /// Returns the last selected lesson
        /// </summary>
        public Lesson SelectedLesson
        {
            get { return selectedLesson; }
            set
            {
                selectedLesson = value;
                NotifyPropertyChanged();
                var lessonSelected = LessonSelected;
                if (lessonSelected != null)
                {
                    lessonSelected(this, new EventArgs());
                }
            }
        }

        SceneViewModel sceneViewModel;
        public SceneViewModel SceneViewModel
        {
            get
            {
                if (sceneViewModel == null)
                {
                    sceneViewModel = new SceneViewModel();
                }
                return sceneViewModel;
            }
        }

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
