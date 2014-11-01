
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkDialectLEarningBLL
{
    public enum Activity
    {
        Learn = 1,
        Practice = 2,
        PlayAll = 3
    }

    public delegate void ActivityChangedEventHandler(object sender, ActivityChangedEventArgs e);

    public class ActivityChangedEventArgs : EventArgs
    {

        public ActivityChangedEventArgs(Activity newActivity, Activity oldActivity)
        {
            NewActivity = newActivity;
            OldActivity = oldActivity;
        }

        public Activity NewActivity { get; private set; }

        public Activity OldActivity { get; private set; }
    }
}
