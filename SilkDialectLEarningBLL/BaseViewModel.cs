using SilkDialectLearningDAL;
using System;
using System.Collections.Generic;
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
            Languages.CollectionChanged += Languages_CollectionChanged;
            PropertyChanged += ViewModel_PropertyChanged;
        }

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Levels")
            {
                Levels.CollectionChanged += Levels_CollectionChanged;
            }
        }


        #region Methods

        /// <summary>
        /// this Method deletes language cascade Language.Levels.Units.Lessons.Scenes.SceneItems
        /// </summary>
        /// <param name="language"></param>
        private int DeleteLanguage(Language language)
        {
            ObservableCollection<Level> levels = language.Levels;
            if (levels.Count > 0)
            {
                foreach (Level level in levels)
                {
                    DeleteLevel(level);
                }
            }
            return Db.Delete(language);
        }

        private int DeleteLevel(Level level)
        {
            ObservableCollection<Unit> units = level.Units;
            if (units.Count > 0)
            {
                foreach (Unit unit in units)
                {
                    DeleteUnit(unit);
                }
            }
            return Db.Delete(level);
        }

        private int DeleteUnit(Unit unit)
        {
            ObservableCollection<Lesson> lessons = unit.Lessons;
            if (lessons.Count > 0)
            {
                foreach (Lesson lesson in lessons)
                {
                    DeleteLesson(lesson);
                }
            }
            return Db.Delete(unit);
        }

        private int DeleteLesson(Lesson lesson)
        {
            IList<Scene> scenes = lesson.Scenes;
            if (scenes.Count > 0)
            {
                foreach (Scene scene in scenes)
                {
                    DeleteScene(scene);
                }
            }
            if (lesson.Vocabularies.Count > 0)
            {

            }
            if (lesson.SentenceBuildings.Count > 0)
            {

            }
            return Db.Delete(lesson);
        }

        private int DeleteScene(Scene scene)
        {
            ObservableCollection<SceneItem> sceneItems = scene.SceneItems;
            if (sceneItems.Count > 0)
            {
                foreach (SceneItem sceneItem in sceneItems)
                {
                    DeleteSceneItem(sceneItem);
                }
            }
            if (scene.ScenePicture != null)
                Db.Delete(scene.ScenePicture);
            return Db.Delete(scene);
        }

        private int DeleteSceneItem(SceneItem sceneItem)
        {
            if (sceneItem.Phrase != null)
                DeletePhrase(sceneItem.Phrase);

            return Db.Delete(sceneItem);
        }

        private int DeletePhrase(Phrase phrase)
        {
            return Db.Delete(phrase);
        }

        private void DeleteVocabualary()
        {
            //TODO: Delete Vocabualary
        }


        /// <summary>
        /// This method works when Languages Collection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Languages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems;
            var oldItems = e.OldItems;
            if (newItems != null)
            {
                if (newItems.Count > 0)
                {
                    Language addedLanguage = (newItems.SyncRoot as object[])[0] as Language;
                    if (addedLanguage != null)
                        Db.Insert(addedLanguage, typeof(Language));
                }
            }
            else if (oldItems != null)
            {
                if (oldItems.Count > 0)
                {
                    Language deletedLanguage = (oldItems.SyncRoot as object[])[0] as Language;
                    int result = DeleteLanguage(deletedLanguage);
                }
            }
        }

        /// <summary>
        /// This method works when Levels Collection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Levels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems;
            var oldItems = e.OldItems;
            if (newItems != null)
            {
                if (newItems.Count > 0)
                {
                    Level addedLevel = (newItems.SyncRoot as object[])[0] as Level;
                    if (addedLevel != null)
                    {
                        this.SelectedLanguage.InsertLevel(addedLevel);
                        NotifyPropertyChanged("Levels");
                    }
                }
            }
            else if (oldItems != null)
            {
                if (oldItems.Count > 0)
                {
                    Level deletedLevel = (oldItems.SyncRoot as object[])[0] as Level;
                    DeleteLevel(deletedLevel);
                    NotifyPropertyChanged("Levels");
                }
            }
        }

        /// <summary>
        /// This method works when Units Collection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Units_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems;
            var oldItems = e.OldItems;
            if (newItems != null)
            {
                if (newItems.Count > 0)
                {
                    Unit addedUnit = (newItems.SyncRoot as object[])[0] as Unit;
                    if (addedUnit != null)
                        this.SelectedLevel.InsertUnit(addedUnit);
                }
            }
            else if (oldItems != null)
            {
                if (oldItems.Count > 0)
                {
                    Unit deletedUnit = (oldItems.SyncRoot as object[])[0] as Unit;
                    DeleteUnit(deletedUnit);
                }
            }
        }

        /// <summary>
        /// This method works when Lesson Collection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lessons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems;
            var oldItems = e.OldItems;
            if (newItems != null)
            {
                if (newItems.Count > 0)
                {
                    Lesson addedLesson = (newItems.SyncRoot as object[])[0] as Lesson;
                    int result = addedLesson.Unit.InsertLesson(addedLesson);
                }
            }
            else if (oldItems != null)
            {
                if (oldItems.Count > 0)
                {
                    Lesson deletedLesson = (oldItems.SyncRoot as object[])[0] as Lesson;
                    DeleteLesson(deletedLesson);
                }
            }
        }


        #endregion

        #region Events
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

        #endregion

        #region Properties
        /// <summary>
        /// Access to the database
        /// </summary>
        internal Entities Db { get; private set; }

        private User user;
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
