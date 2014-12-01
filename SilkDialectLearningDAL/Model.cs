using NAudio.Wave;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SilkDialectLearningDAL
{

    public interface IPlayable
    {
        Guid Id { get; }
        Phrase Phrase { get; }
    }

    public interface IHighlightable
    {
        Guid Id { get; }
    }

    public class Entities : SQLiteConnection
    {

        public Entities(string dbPath, bool createDatabase = false)
            : base(dbPath)
        {
            if (createDatabase)
            {
                this.CreateTable<User>();
                this.CreateTable<Language>();
                this.CreateTable<LanguageToLevel>();
                this.CreateTable<Level>();
                this.CreateTable<LevelToUnit>();
                this.CreateTable<Unit>();
                this.CreateTable<UnitToLesson>();
                this.CreateTable<Lesson>();
                this.CreateTable<LessonToActivity>();
                this.CreateTable<Scene>();
                this.CreateTable<ScenePicture>();
                this.CreateTable<SceneItem>();
                this.CreateTable<Phrase>();
                this.CreateTable<Vocabulary>();
                this.CreateTable<VocabularyToWord>();
                this.CreateTable<Word>();
                this.CreateTable<WordToMeaning>();
                this.CreateTable<Meaning>();
                this.CreateTable<SentenceBuilding>();
                this.CreateTable<SentenceBuildingItemPicture>();
                this.CreateTable<SentenceBuildingItem>();
                this.CreateTable<SentenceToWord>();
                this.CreateTable<SceneType>();
            }
            ModelManager.Db = this;
            if (createDatabase)
            {
                List<SceneType> scenesTypes = new List<SceneType>()
                {
                    new SceneType(){ Id= Guid.NewGuid(), Name = "Vocabulary"},
                    new SceneType(){ Id= Guid.NewGuid(), Name = "Scene"},
                    new SceneType(){ Id= Guid.NewGuid(), Name = "Sentence"}
                };
                ModelManager.Db.InsertAll(scenesTypes, typeof(SceneType));
            }
        }

    }

    public class ModelManager
    {
        static Entities _db;
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

    public enum AudioStatus
    {
        Playing,
        Stopped,
        Paused,
    }

    public class User
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public Guid LastOpenedNode { get; set; }

        public Guid LastOpenedScene { get; set; }

    }

    public class Language
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int InsertLevel(Level level)
        {
            int result = ModelManager.Db.Insert(level, typeof(Level));
            ModelManager.Db.Insert(new LanguageToLevel() { LanguageId = this.Id, LevelId = level.Id }, typeof(LanguageToLevel));
            IsLevelsDirty = true;
            return result;
        }
        [Ignore]
        public bool IsLevelsDirty { get; set; }

        private ObservableCollection<Level> _levels;
        [Ignore]
        public ObservableCollection<Level> Levels
        {
            get
            {
                if (IsLevelsDirty || _levels == null)
                {
                    try
                    {
                        var tempLevels = ModelManager.Db.Query<Level>
                            ("select lev.Id, lev.Name, lev.Description from languagetolevel as ll inner join level as lev on ll.levelid=lev.id where ll.languageid='" + this.Id.ToString() + "'");
                        tempLevels.ForEach((a) => { a.SetLanguage(this); });
                        _levels = new ObservableCollection<Level>(tempLevels);
                        IsLevelsDirty = false;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                return _levels;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class LanguageToLevel
    {
        public Guid LanguageId { get; set; }
        public Guid LevelId { get; set; }
    }

    public class Level
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int InsertUnit(Unit unit)
        {
            int result = ModelManager.Db.Insert(unit, typeof(Unit));
            ModelManager.Db.Insert(new LevelToUnit() { LevelId = this.Id, UnitId = unit.Id }, typeof(LevelToUnit));
            IsUnitsDirty = true;
            return result;
        }

        [Ignore]
        public bool IsUnitsDirty { get; set; }

        ObservableCollection<Unit> _units = null;
        [Ignore]
        public ObservableCollection<Unit> Units
        {
            get
            {
                if (IsUnitsDirty || _units == null)
                {
                    var tempUnits = ModelManager.Db.Query<Unit>("select u.Id, u.Name, u.Description from LevelToUnit as lu inner join unit as u on lu.unitid=u.id where lu.Levelid='" + this.Id.ToString() + "'");
                    tempUnits.ForEach((u) =>
                    {
                        u.SetLevel(this);
                    });
                    _units = new ObservableCollection<Unit>(tempUnits);
                }
                return _units;
            }
        }

        public void SetLanguage(Language language)
        {
            _language = language;
        }
        Language _language;
        [Ignore]
        public Language Language
        {
            get { return _language; }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class LevelToUnit
    {
        public Guid LevelId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class Unit
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public string LevelId { get; set; }

        public int InsertLesson(Lesson lesson)
        {
            int result = ModelManager.Db.Insert(lesson, typeof(Lesson));
            ModelManager.Db.Insert(new UnitToLesson() { UnitId = this.Id, LessonId = lesson.Id }, typeof(UnitToLesson));
            IsLessonsDirty = true;
            return result;
        }
        [Ignore]
        public bool IsLessonsDirty { get; set; }

        ObservableCollection<Lesson> _lessons = null;
        [Ignore]
        public ObservableCollection<Lesson> Lessons
        {
            get
            {
                if (IsLessonsDirty || _lessons == null)
                {
                    var tempLessons = ModelManager.Db.Query<Lesson>("select l.Id, l.Name, l.Description from unittolesson as ul inner join lesson as l on ul.LessonId = l.id where ul.UnitId = '" + this.Id + "'");
                    tempLessons.ForEach((l) =>
                    {
                        l.SetUnit(this);
                    });
                    _lessons = new ObservableCollection<Lesson>(tempLessons);
                }
                return _lessons;
            }
        }
        public void SetLevel(Level level)
        {
            _level = level;
        }

        Level _level;
        [Ignore]
        public Level Level { get { return _level; } }

        public override string ToString()
        {
            return Name;
        }
    }

    public class UnitToLesson
    {
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
    }

    public class Lesson
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        public void SetUnit(Unit unit)
        {
            _unit = unit;
        }

        Unit _unit;
        [Ignore]
        public Unit Unit { get { return _unit; } }

        [Ignore]
        public bool IsScenesDirty { get; set; }
        List<Scene> _scenes = null;
        [Ignore]
        public IList<Scene> Scenes
        {
            get
            {
                if (IsScenesDirty || _scenes == null)
                {
                    var tempScenes = ModelManager.Db.Query<Scene>("select s.id, s.Name, s.Description, s.PictureId, s.SceneOrder, s.SceneType from LessonToActivity as ls " +
                        "inner join Scene as s on ls.SceneId = s.Id where ls.LessonId = '" + this.Id + "' and ls.SceneId is not null");
                    tempScenes.ForEach((l) =>
                    {
                        l.SetLesson(this);
                    });
                    _scenes = new List<Scene>(tempScenes);
                    IsScenesDirty = false;
                }
                return _scenes;
            }
        }

        List<Vocabulary> _vocabularies = null;
        [Ignore]
        public IList<Vocabulary> Vocabularies
        {
            get
            {
                if (_vocabularies == null)
                {
                    var tempVocabularies = ModelManager.Db.Query<Vocabulary>("select v.id, v.Name, v.Description from LessonToActivity as ls " +
                        "inner join Vocabulary as v on ls.VocabularyId = v.Id where ls.LessonId = '" + this.Id + "' and ls.VocabularyId is not null;");
                    tempVocabularies.ForEach((l) =>
                    {
                        l.SetLesson(this);
                    });
                    _vocabularies = new List<Vocabulary>(tempVocabularies);
                }
                return _vocabularies;
            }
        }

        ObservableCollection<SentenceBuilding> _sentenceBuildings = null;
        [Ignore]
        public ObservableCollection<SentenceBuilding> SentenceBuildings
        {
            get
            {
                if (_sentenceBuildings == null)
                {
                    var tempSents = ModelManager.Db.Query<SentenceBuilding>("select s.id, s.Name, s.Description from LessonToActivity as ls " +
                        "inner join SentenceBuilding as s on ls.SentBuildingId = s.Id where ls.LessonId = '" + this.Id + "' and ls.SentBuildingId is not null;");
                    tempSents.ForEach((l) =>
                    {
                        l.SetLesson(this);
                    });
                    _sentenceBuildings = new ObservableCollection<SentenceBuilding>(tempSents);
                }
                return _sentenceBuildings;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class LessonToActivity
    {
        public Guid LessonId { get; set; }
        public Guid VocabularyId { get; set; }
        public Guid SceneId { get; set; }
        public Guid SentBuildingId { get; set; }
    }

    public class Scene
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid PictureId { get; set; }

        private int sceneOrder;
        public int SceneOrder { get { return sceneOrder; } set { sceneOrder = value; } }
        public Guid SceneType { get; set; }

        ScenePicture _scenePicture;
        [Ignore]
        public ScenePicture ScenePicture
        {
            get
            {
                if (_scenePicture == null)
                {
                    _scenePicture = ModelManager.Db.Query<ScenePicture>("select * from ScenePicture where id = '" + PictureId.ToString() + "';").FirstOrDefault();
                }
                return _scenePicture;
            }
            set
            {
                _scenePicture = value;
            }
        }

        ObservableCollection<SceneItem> _sceneItems;
        [Ignore]
        public ObservableCollection<SceneItem> SceneItems
        {
            get
            {
                if (_sceneItems == null)
                {
                    var tempSceneItems = ModelManager.Db.Query<SceneItem>("select * from SceneItem where SceneId = '" + Id.ToString() + "';");
                    tempSceneItems.ForEach((s) =>
                    {
                        s.AlreadyInDb = true;
                        s.SetScene(this);
                    });
                    _sceneItems = new ObservableCollection<SceneItem>(tempSceneItems);
                }
                return _sceneItems;
            }
        }


        public void SetLesson(Lesson lesson)
        {
            _lesson = lesson;
        }
        Lesson _lesson;
        [Ignore]
        public Lesson Lesson { get { return _lesson; } }

        public static Scene GetScene(Guid id, Entities context)
        {
            Scene scene = context.Query<Scene>("SELECT * FROM Scene where Id = '" + id.ToString() + "'").FirstOrDefault();
            return scene;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ScenePicture
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public byte[] Picture { get; set; }
    }

    public class SceneType
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class SceneItem : IPlayable, IHighlightable, INotifyPropertyChanged
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        double _xPos;
        double _yPos;
        public double XPos { get { return _xPos; } set { _xPos = value; NotifyPropertyChanged(); } }
        public double YPos { get { return _yPos; } set { _yPos = value; NotifyPropertyChanged(); } }
        double size;
        public double Size { get { return size; } set { size = value; NotifyPropertyChanged(); } }
        public bool IsRound { get; set; }

        int order;
        public int Order { get { return order; } set { order = value; NotifyPropertyChanged(); } }
        public Guid SceneId { get; set; }
        Guid phraseId;
        public Guid PhraseId { get { return phraseId; } set { phraseId = value; NotifyPropertyChanged(); } }

        [Ignore]
        public bool HasChanges { get; set; }

        [Ignore]
        public bool AlreadyInDb { get; set; }

        Phrase _phrase;
        [Ignore]
        public Phrase Phrase
        {
            get
            {
                if (_phrase == null)
                {
                    _phrase = ModelManager.Db.Query<Phrase>("select * from Phrase where Id = '" + PhraseId.ToString() + "';").FirstOrDefault();
                    if (_phrase != null)
                        _phrase.AlreadyInDb = true;
                }
                return _phrase;
            }
            set
            {
                _phrase = value;
            }
        }

        public void SetScene(Scene scene)
        {
            _scene = scene;
        }

        Scene _scene;

        [Ignore]
        public Scene Scene { get { return _scene; } }


        public override string ToString()
        {
            return Convert.ToInt32(XPos) + " - " + Convert.ToInt32(YPos);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            HasChanges = true;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }

    public partial class Phrase
    {
        public event EventHandler Stopped;

        private void OnStop()
        {
            if (Stopped != null)
                Stopped(this, new EventArgs());
        }
        [Ignore]
        public AudioStatus State { get; protected set; }
    }

    public partial class Phrase : INotifyPropertyChanged
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Text { get; set; }

        byte[] _sound;
        public byte[] Sound
        {
            get { return _sound; }
            set
            {
                _sound = value; NotifyPropertyChanged();
                if (_sound != null) { PrepareAudio(); }
            }
        }

        [Ignore]
        public bool AlreadyInDb { get; set; }

        [Ignore]
        public bool HasChanges { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            HasChanges = true;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        #region NAudio stuff

        [Ignore]
        public TimeSpan SoundLength { get; set; }

        public async Task Play(int playFrom = 0)
        {
            if (_sound == null)
            {
                throw new InvalidOperationException("Phrase.Sound is not initialized yet.");
            }
            await Task.Run(() => 
            {
                if (audioOutput.PlaybackState == PlaybackState.Playing)
                {
                    audioOutput.Stop();
                }
                mp3Reader.CurrentTime = TimeSpan.FromMilliseconds(playFrom);
                audioOutput.Play();
                State = AudioStatus.Playing;
            });
            
        }

        public async Task StopPlaying()
        {
            if (_sound == null)
            {
                throw new InvalidOperationException("Phrase.Sound is not initialized yet.");
            }
            await Task.Run(() => 
            {
                if (audioOutput.PlaybackState != PlaybackState.Stopped)
                {
                    audioOutput.Stop();
                    State = AudioStatus.Stopped;
                }
            });
        }

        private WaveOut audioOutput;
        //private WaveFileReader waveReader;
        private Mp3FileReader mp3Reader;
        void PrepareAudio()
        {
            try
            {
                mp3Reader = new Mp3FileReader(Helper.byteArrayToStream(this.Sound));
                SoundLength = mp3Reader.TotalTime;
                var wc = new WaveChannel32(mp3Reader);
                audioOutput = new WaveOut();
                audioOutput.Init(wc);
            }
            catch (Exception ex)
            {
                throw new Exception("There was a problem with preparing the audio file. Please see the inner exception for more details.", ex);
            }
        }
        #endregion

    }

    public class Vocabulary
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IList<Word> Words = new List<Word>();

        public void SetLesson(Lesson lesson)
        {
            _lesson = lesson;
        }
        Lesson _lesson;
        [Ignore]
        public Lesson Lesson
        {
            get
            {
                return _lesson;
            }
        }
    }

    public class VocabularyToWord
    {
        public Guid VocabularyId { get; set; }
        public Guid WordId { get; set; }
        public bool DoNotIncludeToExam { get; set; }
    }

    public class Word : INotifyPropertyChanged
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public byte[] Sound { get; set; }
        //public int SoundVol { get; set; }

        Guid phraseId;
        public Guid PhraseId { get { return phraseId; } set { phraseId = value; NotifyPropertyChanged(); } }

        Phrase _phrase;
        [Ignore]
        public Phrase Phrase
        {
            get
            {
                if (_phrase == null)
                {
                    _phrase = ModelManager.Db.Query<Phrase>("select * from Phrase where Id = '" + PhraseId.ToString() + "';").FirstOrDefault();
                    if (_phrase != null)
                        _phrase.AlreadyInDb = true;
                }
                return _phrase;
            }
            set
            {
                _phrase = value;
            }
        }

        public void SetVocabulary(Vocabulary vocabulary)
        {
            _vocabulary = vocabulary;
        }

        Vocabulary _vocabulary;
        [Ignore]
        public Vocabulary Vocabulary { get { return _vocabulary; } }


        ObservableCollection<Meaning> _meanings = null;
        [Ignore]
        public ObservableCollection<Meaning> Meanings
        {
            get
            {
                return _meanings;
            }
        }

        #region Notify

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

    public class WordToMeaning
    {
        public Guid WordId { get; set; }
        public Guid MeaningId { get; set; }
    }

    public class Meaning
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }

        public IList<Word> Words = new List<Word>();
    }

    public class SentenceBuilding
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IList<SentenceBuildingItem> SentenceBuildingItems = new List<SentenceBuildingItem>();

        public void SetLesson(Lesson lesson)
        {
            _lesson = lesson;
        }
        Lesson _lesson;
        [Ignore]
        public Lesson Lesson { get { return _lesson; } }
    }

    public class SentenceBuildingItemPicture
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }
    }

    public class SentenceBuildingItem
    {
        [PrimaryKey]
        public int Guid { get; set; }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public int SentenceBuildingItemPictureId
        {
            get;
            set;
        }

        public int PhraseId
        {
            get;
            set;
        }
        [Ignore]
        public SentenceBuildingItemPicture Picture { get; set; }
        [Ignore]
        public Phrase Phrase { get; set; }
        public IList<Word> Words = new List<Word>();

    }

    public class SentenceToWord
    {
        public Guid SentenceBuildingItemId { get; set; }
        public Guid WordId { get; set; }
    }

    public class Helper
    {
        public static Stream byteArrayToStream(Byte[] bytes)
        {
            Stream str = new MemoryStream(bytes);
            return str;
        }
    }
}
