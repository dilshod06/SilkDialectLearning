using System;

namespace SilkDialectLearningBLL
{
    public class Global
    {
        public static string DatabasePath { get; set; }

        public static bool CreateDatabase { get; set; }

        static ViewModel globalViewModel;
        public static ViewModel GlobalViewModel
        {
            get
            {
                if (globalViewModel == null)
                {
                    globalViewModel = new ViewModel(DatabasePath, "", "", CreateDatabase);
                }
                return globalViewModel;
            }
            set
            {
                if (globalViewModel != null)
                {
                    throw new Exception("Can't set GlobalViewModel twice");
                }
                globalViewModel = value;
            }
        }
    }
}
