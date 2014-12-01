using SQLite;

namespace SilkDialectLearningDAL
{
    public class DataProvider
    {
        string _pathToDatabase;

        public DataProvider()
        {
            _pathToDatabase = "../../../Assets/Lang.db";
            using (var conn = new SQLite.SQLiteConnection(_pathToDatabase))
            {
                //conn.CreateTable<MyItem>();
            }
        }

        /// <summary>
        /// Global way to grab a connection to the database, make sure to wrap in a using
        /// </summary>
        public SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(_pathToDatabase, true);
            //TODO: Get the initialized check to work
            /*
            if (!initialized)
            {
                CreateDatabase(connection, cancellationToken).Wait();
            }
            */
            return connection;
        }

    }
}
