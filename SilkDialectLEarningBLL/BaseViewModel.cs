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

        //TODO: Look all of your delete methods are almost identical. Create one method and it should call itself to delete its children. 
        /// <summary>
        /// This method deletes selectedLanguage cascade Language.Levels.Units.Lessons.Scenes.SceneItems
        /// </summary>
        /// <param name="selectedLanguage">Language</param>
        public async Task<int> DeleteLanguage(Language selectedLanguage)
        {
            ObservableCollection<Level> levels = selectedLanguage.Levels;
            if (levels.Count > 0)//TODO: Since you are doing foreach no need to check for levels.Count!=0
            {
                foreach (Level level in levels)
                {
                    await DeleteLevel(level);
                }
            }
            return await Db.SqLiteAsyncConnection.DeleteAsync(selectedLanguage);
        }

        public async Task<int> DeleteLevel(Level level)
        {
            ObservableCollection<Unit> units = level.Units;
            if (units.Count > 0)
            {
                foreach (Unit unit in units)
                {
                    await DeleteUnit(unit);
                }
            }
            SelectedLanguage.IsLevelsDirty = true;
            return await Db.SqLiteAsyncConnection.DeleteAsync(level);
        }

        public async Task<int> DeleteUnit(Unit unit)
        {
            ObservableCollection<Lesson> lessons = unit.Lessons;
            if (lessons.Count > 0)
            {
                foreach (Lesson lesson in lessons)
                {
                    await DeleteLesson(lesson);
                }
            }
            SelectedLevel.IsUnitsDirty = true;
            return await Db.SqLiteAsyncConnection.DeleteAsync(unit);
        }

        public async Task<int> DeleteLesson(Lesson lesson)
        {
            IList<Scene> scenes = lesson.Scenes;
            if (scenes.Count > 0)
            {
                foreach (Scene scene in scenes)
                {
                    await DeleteScene(scene);
                }
            }
            if (lesson.Vocabularies.Count > 0)
            {

            }
            if (lesson.SentenceBuildings.Count > 0)
            {

            }
            SelectedUnit.IsLessonsDirty = true;
            return await Db.SqLiteAsyncConnection.DeleteAsync(lesson);
        }

        public async Task<int> DeleteScene(Scene scene)
        {
            ObservableCollection<SceneItem> sceneItems = scene.SceneItems;
            if (sceneItems.Count > 0)
            {
                foreach (SceneItem sceneItem in sceneItems)
                {
                    await DeleteSceneItem(sceneItem);
                }
            }

            if (scene.ScenePicture != null)
                await Db.SqLiteAsyncConnection.DeleteAsync(scene.ScenePicture);
            return await Db.SqLiteAsyncConnection.DeleteAsync(scene);
        }

        public async Task<int> DeleteSceneItem(SceneItem sceneItem)
        {
            if (sceneItem.Phrase != null)
                await DeletePhrase(sceneItem.Phrase);

            return await Db.SqLiteAsyncConnection.DeleteAsync(sceneItem);
        }

        private async Task<int> DeletePhrase(Phrase phrase)
        {
            return await Db.SqLiteAsyncConnection.DeleteAsync(phrase);
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

        public void DeleteVocabualary()
        {
            //TODO: Delete Vocabualary
        }

        public Task<int> Update(object entity) 
        {
            return Db.SqLiteAsyncConnection.UpdateAsync(entity);
        }

        public Task<int> UpdateAll(IEnumerable items)
        {
            return Db.SqLiteAsyncConnection.UpdateAllAsync(items);
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

    public class LoadingEventArgs : EventArgs
    {
        public bool Loading { get; set; }
        public string Message { get; set; }

        public LoadingEventArgs(bool loading, string message)
        {
            Loading = loading;
            Message = message;
        }
    }
}
