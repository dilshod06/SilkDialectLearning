using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SilkDialectLearning.DAL;
using SQLite.Net.Interop;
using SQLiteNetExtensions.Extensions;

namespace SilkDialectLearning.BLL
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel(User user)
        {
            User = user;
        }

        public ViewModel(
            ISQLitePlatform sqlitePlatform, // When user uses another platform it will take X platform instance, for example Win32, Android, iOS, Windows Phone, Windows Store App platforms 
            IAudioManager audioManager, // When user uses another platform will be used because, every platform has other methods to play audio 
            string dbPath, 
            string userName = "", 
            string password = "", 
            bool createDatabase = false, 
            User user = null)
        {
            User = user;
            AudioManager = audioManager;
            var db = new Entities(sqlitePlatform, dbPath, createDatabase);
            Db = db;
        }

        #region Properties
        /// <summary>
        /// Access to Play audio and StopAudio
        /// </summary>
        public IAudioManager AudioManager { get; set; }


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
                languages = new ObservableCollection<Language>(Db.Table<Language>().ToList());
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
                if (value != null)
                {
                    language = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Levels");
                    var languageSelected = LanguageSelected;
                    if (languageSelected != null)
                    {
                        languageSelected(this, new EventArgs());
                    }
                    return;
                }
                language = value;
                NotifyPropertyChanged();
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
                    try
                    {
                        levels = new ObservableCollection<Level>
                        (
                            Db.GetWithChildren<Language>(SelectedLanguage.Id).Levels
                        );
                    }
                    catch (Exception)
                    {
                    }
                    
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
                    try
                    {
                        units = new ObservableCollection<Unit>
                        (
                            Db.GetWithChildren<Level>(SelectedLevel.Id).Units
                        );
                    }
                    catch (Exception)
                    {
                    }
                        
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
                    try
                    {
                        lessons = new ObservableCollection<Lesson>
                        (
                            Db.GetWithChildren<Unit>(SelectedUnit.Id).Lessons
                        );
                    }
                    catch (Exception)
                    {
                    }
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
            get { return sceneViewModel ?? (sceneViewModel = new SceneViewModel()); }
        }
        #endregion

        #region Methods

        public Task<int> AsyncDelete(IEntity entity, bool recurcive = false)
        {
            return Task.Factory.StartNew(() =>
            {
                var conn = Db.GetConnectionWithLock();
                using (conn.Lock())
                {
                    // If user want delete Entity with all childrens(recurcive) first it should get entity with childrens
                    try
                    {
                        if (!recurcive)
                        {
                            try
                            {
                                conn.Delete(entity);
                                Db.Vacuum();
                            }
                            catch (Exception)
                            {
                                return 0;
                            }
                            return 1;
                        }
                        IEntity entityWithChildren = null;
                        if (entity is Language)
                        {
                            entityWithChildren = conn.GetWithChildren<Language>(entity.Id, true);
                            // if recurcive will be true deletes entity with all childrens else deletes only entity
                            conn.Delete(entityWithChildren, recurcive);
                            NotifyPropertyChanged("Languages");
                            if (recurcive)
                            {
                                NotifyPropertyChanged("Levels");
                                NotifyPropertyChanged("Units");
                                NotifyPropertyChanged("Lessons");
                                SceneViewModel.NotifyPropertyChanged("Scenes");
                                SceneViewModel.NotifyPropertyChanged("SceneItems");
                            }
                        }
                        else if (entity is Level)
                        {
                            entityWithChildren = conn.GetWithChildren<Level>(entity.Id, true);
                            conn.Delete(entityWithChildren, recurcive);
                            NotifyPropertyChanged("Levels");
                            if (recurcive)
                            {
                                NotifyPropertyChanged("Units");
                                NotifyPropertyChanged("Lessons");
                                SceneViewModel.NotifyPropertyChanged("Scenes");
                                SceneViewModel.NotifyPropertyChanged("SceneItems");
                            }
                        }
                        else if (entity is Unit)
                        {
                            entityWithChildren = conn.GetWithChildren<Unit>(entity.Id, true);
                            conn.Delete(entityWithChildren, recurcive);
                            NotifyPropertyChanged("Units");
                            if (recurcive)
                            {
                                NotifyPropertyChanged("Lessons");
                                SceneViewModel.NotifyPropertyChanged("Scenes");
                                SceneViewModel.NotifyPropertyChanged("SceneItems");
                            }
                        }
                        else if (entity is Lesson)
                        {
                            entityWithChildren = conn.GetWithChildren<Lesson>(entity.Id, true);
                            conn.Delete(entityWithChildren, recurcive);
                            NotifyPropertyChanged("Lessons");
                            if (recurcive)
                            {
                                SceneViewModel.NotifyPropertyChanged("Scenes");
                                SceneViewModel.NotifyPropertyChanged("SceneItems");
                            }
                        }
                        else if (entity is Scene)
                        {
                            entityWithChildren = conn.GetWithChildren<Scene>(entity.Id, true);
                            conn.Delete(entityWithChildren, recurcive);
                            SceneViewModel.NotifyPropertyChanged("Scenes");
                            if (recurcive)
                            {
                                SceneViewModel.NotifyPropertyChanged("SceneItems");
                            }
                        }
                        Db.Vacuum();
                        return 1;
                    }
                    catch (Exception)
                    {
                        return 0;
                    }
                }
            });
        }

        public Task<int> AsyncDeleteAll(IEnumerable entities, bool recurcive = false)
        {
            return Task.Factory.StartNew(() =>
            {
                var conn = Db.GetConnectionWithLock();

                using (conn.Lock())
                {
                    conn.DeleteAll(entities, recurcive);
                    return 1;
                }
            });
        }

        public Task<int> Update(object item)
        {
            return Db.SqLiteAsyncConnection.UpdateAsync(item);
        }

        public Task<int> UpdateAll(IEnumerable items)
        {
            return Db.SqLiteAsyncConnection.UpdateAllAsync(items);
        }

        public Task<int> AsyncInsertEntity(IEntity entity)
        {
            return Task.Factory.StartNew(() =>
            {
                var conn = Db.GetConnectionWithLock();
                using (conn.Lock())
                {
                    if (entity is Language)
                    {
                        return conn.Insert(entity);
                    }
                    else if (entity is Level)
                    {
                        SelectedLanguage.Levels.Add(entity as Level);
                        conn.InsertOrReplaceWithChildren(SelectedLanguage, true);
                        return 1;
                    }
                    else if (entity is Unit)
                    {
                        SelectedLevel.Units.Add(entity as Unit);
                        conn.InsertOrReplaceWithChildren(SelectedLevel, true);
                        return 1;
                    }
                    else if (entity is Lesson)
                    {
                        SelectedUnit.Lessons.Add(entity as Lesson);
                        conn.InsertOrReplaceWithChildren(SelectedUnit, true);
                        return 1;
                    }
                }
                return 0;
            });
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
