using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace SilkDialectLearning.DAL
{
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
        private List<Level> levels = new List<Level>();

        [ManyToMany(typeof(LanguageToLevel), CascadeOperations = CascadeOperation.All)]
        public List<Level> Levels
        {
            get { return levels; }
            set { levels = value; }
        }

        ~Language()
        {
            Dispose();
        }
    }

    public class LanguageToLevel
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
        private List<Unit> units = new List<Unit>();

        [ManyToMany(typeof(LevelToUnit), CascadeOperations = CascadeOperation.All)]
        public List<Unit> Units
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

    public class LevelToUnit
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
        private List<Lesson> lessons = new List<Lesson>();

        [ManyToMany(typeof(UnitToLesson), CascadeOperations = CascadeOperation.All)]
        public List<Lesson> Lessons
        {
            get { return lessons; }
            set { lessons = value; NotifyPropertyChanged(); }
        }

        ~Unit()
        {
            Dispose();
        }
    }

    public class UnitToLesson
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
        private List<Scene> scenes = new List<Scene>();

        /// <summary>
        /// Gets and Sets this Lessons Scene List 
        /// </summary>
        [ManyToMany(typeof(LessonToActivity), CascadeOperations = CascadeOperation.All)]
        public List<Scene> Scenes
        {
            get { return scenes; }
            set { scenes = value; NotifyPropertyChanged(); }
        }

        private List<Vocabulary> vocabularies = new List<Vocabulary>();

        /// <summary>
        /// Gets and sets this Lessons Vocabulary List
        /// </summary>
        [ManyToMany(typeof(LessonToActivity), CascadeOperations = CascadeOperation.All)]
        public List<Vocabulary> Vocabularies
        {
            get { return vocabularies; }
            set { vocabularies = value; NotifyPropertyChanged(); }
        }

        private List<SentenceBuilding> sentenceBuildings = new List<SentenceBuilding>();

        [ManyToMany(typeof(LessonToActivity), CascadeOperations = CascadeOperation.All)]
        public List<SentenceBuilding> SentenceBuildings
        {
            get { return sentenceBuildings; }
            set { sentenceBuildings = value; NotifyPropertyChanged(); }
        }

        ~Lesson()
        {
            Dispose();
        }
    }

    public class LessonToActivity
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
        public Guid SentBuildingId { get; set; }

    }

    public class Scene : BaseEntity
    {
        [ForeignKey(typeof(ScenePicture))]
        public Guid PictureId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public ScenePicture ScenePicture { get; set; }

        private List<SceneItem> sceneItems = new List<SceneItem>();

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<SceneItem> SceneItems
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

        private TimeSpan soundLength;
        [Ignore]
        public TimeSpan SoundLength 
        {
            get
            {
                return soundLength; 
            }
            set
            {
                soundLength = value; NotifyPropertyChanged();
            } 
        }

        ~Phrase()
        {
            Dispose();
        }
    }

    public class Vocabulary : BaseEntity
    {
        private List<Word> words = new List<Word>();

        [ManyToMany(typeof(VocabularyToWord), CascadeOperations = CascadeOperation.All)]
        public List<Word> Words
        {
            get { return words; }
            set { words = value; }
        }
    }

    public class VocabularyToWord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Vocabulary))]
        public Guid VocabularyId { get; set; }

        [ForeignKey(typeof(Word))]
        public Guid WordId { get; set; }

        public bool DoNotIncludeToExam { get; set; }
    }

    public class Word : BaseEntity, IPlayable, IHighlightable
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

        List<Meaning> meanings = new List<Meaning>();
        /// <summary>
        /// Gets and sets this words meaning Collection
        /// </summary>
        [ManyToMany(typeof(WordToMeaning), CascadeOperations = CascadeOperation.All)]
        public List<Meaning> Meanings
        {
            get { return meanings; }
            set { meanings = value; NotifyPropertyChanged(); }
        }
    }

    public class WordToMeaning
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
        /// <summary>
        /// Gets and sets picture in blob format
        /// </summary>
        public byte[] Picture { get; set; }

        List<Word> words = new List<Word>();

        [ManyToMany(typeof(WordToMeaning), CascadeOperations = CascadeOperation.All)]
        public List<Word> Words
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

        List<SentenceBuildingItem> sentenceBuildingItems = new List<SentenceBuildingItem>();

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<SentenceBuildingItem> SentenceBuildingItems
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

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public SentenceBuildingItemPicture SentenceBuildingItemPicture { get; set; }

        [ForeignKey(typeof(Phrase))]
        public Guid PhraseId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public Phrase Phrase { get; set; }

        List<Word> words = new List<Word>();

        [ManyToMany(typeof(SentenceToWord), CascadeOperations = CascadeOperation.All)]
        public List<Word> Words
        {
            get { return words; }
            set { words = value; NotifyPropertyChanged(); }
        }
    }

    public class SentenceToWord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(SentenceBuildingItem))]
        public Guid SentenceBuildingItemId { get; set; }

        [ForeignKey(typeof(Word))]
        public Guid WordId { get; set; }
    }

}
