using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Extensions;

namespace SilkDialectLearningDAL
{
    public interface IEntity
    {
        Guid Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }

    }
    public interface IPlayable
    {
        Phrase Phrase { get; }
    }

    public interface IHighlightable
    {
        bool IsRound { get; set; }
    }

    public class Entities : SQLiteConnection
    {
        private readonly SQLiteConnectionString connectionParameters;

        private readonly SQLiteConnectionPool sqliteConnectionPool;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public SQLiteAsyncConnection SQLiteAsyncConnection { get; set; }

        public Entities(ISQLitePlatform sqlitePlatform, string databasePath, bool createDatabase = false)
            : base(sqlitePlatform, databasePath)
        {
            connectionParameters = new SQLiteConnectionString(databasePath, false);
            sqliteConnectionPool = new SQLiteConnectionPool(sqlitePlatform);
            SQLiteAsyncConnection = new SQLiteAsyncConnection(() => sqliteConnectionPool.GetConnection(connectionParameters));
            if (createDatabase)
            {
                CreateTable<User>();
                CreateTable<Language>();
                CreateTable<LanguageToLevel>();
                CreateTable<Level>();
                CreateTable<LevelToUnit>();
                CreateTable<Unit>();
                CreateTable<UnitToLesson>();
                CreateTable<Lesson>();
                CreateTable<LessonToActivity>();
                CreateTable<Scene>();
                CreateTable<ScenePicture>();
                CreateTable<SceneItem>();
                CreateTable<Phrase>();
                CreateTable<Vocabulary>();
                CreateTable<VocabularyToWord>();
                CreateTable<Word>();
                CreateTable<WordToMeaning>();
                CreateTable<Meaning>();
                CreateTable<SentenceBuilding>();
                CreateTable<SentenceBuildingItemPicture>();
                CreateTable<SentenceBuildingItem>();
                CreateTable<SentenceToWord>();
            }
            ModelManager.Db = this;
        }
    }

    public static class ModelManager
    {
        private static Entities _db;

        public static Entities Db
        {
            get
            {
                if (_db == null)
                    throw new Exception("Instance of MainEntities not created yet.");
                return _db;
            }
            set
            {
                _db = value;
            }
        }
    }


    public abstract class BaseEntity : IEntity, INotifyPropertyChanged, IDisposable
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ReSharper disable once MemberCanBeProtected.Global
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class User : BaseEntity
    {
    }

    public class Language : BaseEntity
    {
        private ObservableCollection<Level> levels = new ObservableCollection<Level>();

        [ManyToMany(typeof(LanguageToLevel), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Level> Levels
        {
            get { return levels; }
            set { levels = value; }
        }

        ~Language()
        {
            Dispose();
        }
    }

    public abstract class LanguageToLevel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Language))]
        public Guid LanguageId { get; set; }

        [ForeignKey(typeof(Level))]
        public Guid LevelId { get; set; }
    }

    public class Level : BaseEntity
    {
        private ObservableCollection<Unit> units = new ObservableCollection<Unit>();

        [ManyToMany(typeof(LevelToUnit), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Unit> Units
        {
            get { return units; }
            set { units = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Distructor of Level
        /// </summary>
        ~Level()
        {
            Dispose();
        }
    }

    public abstract class LevelToUnit
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Level))]
        public Guid LevelId { get; set; }

        [ForeignKey(typeof(Unit))]
        public Guid UnitId { get; set; }
    }

    public class Unit : BaseEntity
    {
        private ObservableCollection<Lesson> lessons = new ObservableCollection<Lesson>();

        [ManyToMany(typeof(UnitToLesson), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Lesson> Lessons
        {
            get { return lessons; }
            set { lessons = value; NotifyPropertyChanged(); }
        }

        ~Unit()
        {
            Dispose();
        }
    }

    public abstract class UnitToLesson
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Unit))]
        public Guid UnitId { get; set; }

        [ForeignKey(typeof(Lesson))]
        public Guid LessonId { get; set; }
    }

    public class Lesson : BaseEntity
    {
        private ObservableCollection<Scene> scenes = new ObservableCollection<Scene>();

        /// <summary>
        /// Gets and Sets this Lessons Scene List 
        /// </summary>
        [ManyToMany(typeof(LessonToActivity), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Scene> Scenes
        {
            get { return scenes; }
            set { scenes = value; NotifyPropertyChanged(); }
        }


        private ObservableCollection<Vocabulary> vocabularies = new ObservableCollection<Vocabulary>();

        /// <summary>
        /// Gets and sets this Lessons Vocabulary List
        /// </summary>
        [ManyToMany(typeof(LessonToActivity), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Vocabulary> Vocabularies
        {
            get { return vocabularies; }
            set { vocabularies = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<SentenceBuilding> sentenceBuildings = new ObservableCollection<SentenceBuilding>();

        [ManyToMany(typeof(LessonToActivity), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<SentenceBuilding> SentenceBuildings
        {
            get { return sentenceBuildings; }
            set { sentenceBuildings = value; NotifyPropertyChanged(); }
        }

        ~Lesson()
        {
            Dispose();
        }
    }

    public abstract class LessonToActivity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Lesson))]
        public Guid LessonId { get; set; }

        [ForeignKey(typeof(Scene))]
        public Guid SceneId { get; set; }

        [ForeignKey(typeof(Vocabulary))]
        public Guid VocabularyId { get; set; }

        [ForeignKey(typeof(SentenceBuilding))]
        public Guid SentenceBuilding { get; set; }
    }

    public class Scene : BaseEntity
    {
        [ForeignKey(typeof(ScenePicture))]
        public Guid PictureId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ScenePicture ScenePicture { get; set; }

        private ObservableCollection<SceneItem> sceneItems = new ObservableCollection<SceneItem>();

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<SceneItem> SceneItems
        {
            get { return sceneItems; }
            set { sceneItems = value; }
        }

        ~Scene()
        {
            Dispose();
        }
    }

    public class ScenePicture : BaseEntity
    {
        [Ignore]
        public new string Name { get; set; }

        [Ignore]
        public new string Description { get; set; }

        public byte[] Picture { get; set; }
    }

    public class SceneItem : BaseEntity, IHighlightable, IPlayable
    {
        [Ignore]
        public new string Name { get; set; }

        [Ignore]
        public new string Description { get; set; }

        private double xPos;
        public double XPos
        {
            get { return xPos; }
            set { xPos = value; NotifyPropertyChanged(); }
        }

        private double yPos;
        public double YPos
        {
            get { return yPos; }
            set { yPos = value; NotifyPropertyChanged(); }
        }

        private double size;
        public double Size
        {
            get { return size; }
            set { size = value; NotifyPropertyChanged(); }
        }

        public bool IsRound { get; set; }

        private int order;
        public int Order
        {
            get { return order; }
            set { order = value; NotifyPropertyChanged(); }
        }

        [ForeignKey(typeof(Scene))]
        public Guid SceneId { get; set; }

        [ManyToOne]
        public Scene Scene { get; set; }

        [ForeignKey(typeof(Phrase))]
        public Guid PhraseId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Phrase Phrase { get; set; }

        ~SceneItem()
        {
            Dispose();
        }
    }

    public class Phrase : BaseEntity
    {
        [Ignore]
        public new string Name { get; set; }

        [Ignore]
        public new string Description { get; set; }

        private byte[] sound;
        public byte[] Sound
        {
            get { return sound; }
            set
            {
                sound = value;
                NotifyPropertyChanged();
            }
        }

        ~Phrase()
        {
            Dispose();
        }
    }

    public class Vocabulary : BaseEntity
    {
        private ObservableCollection<Word> words = new ObservableCollection<Word>();

        [ManyToMany(typeof(VocabularyToWord), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Word> Words
        {
            get { return words; }
            set { words = value; }
        }
    }

    public abstract class VocabularyToWord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Vocabulary))]
        public Guid VocabularyId { get; set; }

        [ForeignKey(typeof(Word))]
        public Guid WordId { get; set; }

        public bool DoNotIncludeToExam { get; set; }
    }

    public class Word : BaseEntity
    {
        private Guid phraseId;
        /// <summary>
        /// Gets and sets phrase id
        /// </summary>
        [ForeignKey(typeof(Phrase))]
        public Guid PhraseId
        {
            get { return phraseId; }
            set { phraseId = value; }
        }

        /// <summary>
        /// Gets and sets this word's phrase
        /// </summary>
        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public Phrase Phrase { get; set; }

        /// <summary>
        /// Gets and sets picture in blob format
        /// </summary>
        public byte[] Picture { get; set; }

        ObservableCollection<Meaning> meanings = new ObservableCollection<Meaning>();
        /// <summary>
        /// Gets and sets this words meaning Collection
        /// </summary>
        [ManyToMany(typeof(WordToMeaning), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Meaning> Meanings
        {
            get { return meanings; }
            set { meanings = value; NotifyPropertyChanged(); }
        }
    }

    public abstract class WordToMeaning
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Word))]
        public Guid WordId { get; set; }

        [ForeignKey(typeof(Meaning))]
        public Guid MeaningId { get; set; }
    }

    public class Meaning : BaseEntity
    {
        ObservableCollection<Word> words = new ObservableCollection<Word>();

        [ManyToMany(typeof(WordToMeaning), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Word> Words
        {
            get { return words; }

            set { words = value; NotifyPropertyChanged(); }
        }
    }

    /// <summary>
    ///  This class will be used for 
    /// </summary>
    public class SentenceBuilding : BaseEntity
    {
        ObservableCollection<SentenceBuildingItem> sentenceBuildingItems = new ObservableCollection<SentenceBuildingItem>();

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<SentenceBuildingItem> SentenceBuildingItems
        {
            get { return sentenceBuildingItems; }
            set { sentenceBuildingItems = value; NotifyPropertyChanged(); }
        }

    }

    public class SentenceBuildingItemPicture : BaseEntity
    {
        public byte[] Picture { get; set; }

    }

    /// <summary>
    /// Sentence Bulding will be used for Sentence Buildings collection
    /// </summary>
    public class SentenceBuildingItem : BaseEntity
    {
        [ForeignKey(typeof(SentenceBuilding))]
        public Guid SentenceBuildingId { get; set; }

        [ManyToOne]
        public SentenceBuilding SentenceBuilding { get; set; }

        [ForeignKey(typeof(SentenceBuildingItemPicture))]
        public Guid SentenceBuildingItemPictureId { get; set; }

        [OneToOne]
        public SentenceBuildingItemPicture SentenceBuildingItemPicture { get; set; }

        [ForeignKey(typeof(Phrase))]
        public Guid PhraseId { get; set; }

        [OneToOne]
        public Phrase Phrase { get; set; }

        ObservableCollection<Word> words = new ObservableCollection<Word>();

        [ManyToMany(typeof(SentenceToWord), CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Word> Words
        {
            get { return words; }
            set { words = value; NotifyPropertyChanged(); }
        }
    }

    public abstract class SentenceToWord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(SentenceBuildingItem))]
        public Guid SentenceBuildingItemId { get; set; }

        [ForeignKey(typeof(Word))]
        public Guid WordId { get; set; }
    }

}
