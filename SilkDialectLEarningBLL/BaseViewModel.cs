using SilkDialectLearningDAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SilkDialectLearningBLL
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel(User user)
        {
            User = user;
        }

        public ViewModel(string dbPath, string userName = "", string password = "", bool createDatabase = false, User user = null)
        {
            User = user;
            var db = new Entities(dbPath, createDatabase);
            Db = db;
        }

        #region Properties
        /// <summary>
        /// Access to the database
        /// </summary>
        internal Entities Db { get; private set; }

        private User user;
        /// <summary>
        /// Returns the current logged in user
        /// </summary>
        public User User { get { return user; } private set { user = value; NotifyPropertyChanged(); } }

        ObservableCollection<Language> languages;

        /// <summary>
        /// Returns existing languages in the database 
        /// </summary>
        public ObservableCollection<Language> Languages
        {
            get
            {
                languages = new ObservableCollection<Language>(Db.GetEntities<Language>());
                return languages;
            }
        }

        Language language;
        /// <summary>
        /// Returns the last selected selectedLanguage
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

        ObservableCollection<Level> levels = new ObservableCollection<Level>();
        /// <summary>
        /// Returns the list of levels under selected selectedLanguage
        /// </summary>
        public ObservableCollection<Level> Levels
        {
            get
            {
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

        private SceneViewModel sceneViewModel;
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
        #endregion

        #region Methods

        /// <summary>
        /// This method deletes selectedLanguage cascade Language.Levels.Units.Lessons.Scenes.SceneItems
        /// </summary>
        /// <param name="selectedLanguage">Language</param>
        public async Task<int> Delete(IEntity entity)
        {
            IEnumerable<IEntity> childEntities = null;
            Language language = entity as Language;
            Level level = entity as Level;
            Unit unit = entity as Unit;
            Lesson lesson = entity as Lesson;
            Scene scene = entity as Scene;
            SceneItem sceneItem = entity as SceneItem;
            Phrase phrase = entity as Phrase;
            if (language != null)
            {
                childEntities = language.Levels;
                //var langToLevel = Db.Query<LanguageToLevel>("select * from LanguageToLevel where LanguageId = '" + language.Id + "'");
                //await Db.SqLiteAsyncConnection.DeleteAsync(langToLevel);
            }
            else if (level != null)
            {
                childEntities = level.Units;
            }
            else if (unit != null)
            {
                childEntities = unit.Lessons;
            }
            else if (lesson != null)
            {
                childEntities = lesson.Scenes;
            }
            else if (scene != null)
            {
                childEntities = scene.SceneItems;
            }
            else if (sceneItem != null)
            {
                if (sceneItem.Phrase != null)
                    await Delete(sceneItem.Phrase);
            }
            if (childEntities != null)
            {
                foreach (var childEntity in childEntities)
                {
                    await Delete(childEntity);
                }
            }
            return await Db.SqLiteAsyncConnection.DeleteAsync(entity);
        }

        public Task<int> Update(object item)
        {
            return Db.SqLiteAsyncConnection.UpdateAsync(item);
        }

        public Task<int> UpdateAll(IEnumerable items)
        {
            return Db.SqLiteAsyncConnection.UpdateAllAsync(items);
        }

        //TODO: This method looks good, but I would say it should be in DAL. What do you think?
        public Task<int> InsertEntity(IEntity entity)
        {
            if (entity is Language)
            {
                return Db.SqLiteAsyncConnection.InsertAsync(entity);
            }
            else if (entity is Level)
            {
                return SelectedLanguage.InsertLevel(entity as Level);

            }
            else if (entity is Unit)
            {
                return selectedLevel.InsertUnit(entity as Unit);
            }
            else if (entity is Lesson)
            {
                return SelectedUnit.InsertLesson(entity as Lesson);
            }
            return new Task<int>(() => 0);
        }

        #endregion

        #region Events
        /// <summary>
        /// Will be raised when a selectedLanguage gets selected
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

        #endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

}
