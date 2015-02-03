using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Interop;

namespace SilkDialectLearningDAL
{
    public class Entities : SQLiteConnection
    {
        private readonly ISQLitePlatform sqlitePlatform;

        private readonly SQLiteConnectionString connectionParameters;

        private readonly SQLiteConnectionPool sqliteConnectionPool;

        /// <summary>
        /// Gets and sets SQLiteAsync connection for awaitable stuffs
        /// </summary>
        public SQLiteAsyncConnection SQLiteAsyncConnection { get; set; }

        public Entities(ISQLitePlatform sqlitePlatform, string databasePath, bool createDatabase = false)
            : base(sqlitePlatform, databasePath)
        {
            this.sqlitePlatform = sqlitePlatform;
            connectionParameters = new SQLiteConnectionString(databasePath, false);
            sqliteConnectionPool = new SQLiteConnectionPool(sqlitePlatform);
            SQLiteAsyncConnection = new SQLiteAsyncConnection(() => GetConnectionWithLock());
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

        public void Vacuum()
        {
            SQLiteCommand cmd = this.CreateCommand("VACUUM;");
            cmd.ExecuteNonQuery();
        }

        public SQLiteConnectionWithLock GetConnectionWithLock()
        {
            return sqliteConnectionPool.GetConnection(connectionParameters);
        }
    }
}
