using System;

namespace SilkDialectLearningDAL
{
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
}
