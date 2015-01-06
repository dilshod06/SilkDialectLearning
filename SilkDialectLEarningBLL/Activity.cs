
using System;

namespace SilkDialectLearningBLL
{
    /// <summary>
    /// Activity types
    /// </summary>
    public enum Activity
    {
        Learn = 1,
        Practice = 2,
        PlayAll = 3
    }

    public enum ViewModelActivity
    {
        SceneViewModel = 1,
        VocabViewModel = 2,
        SentenceViewModel = 3
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

    public delegate void ViewModelActivityChangeHandler(object sender, ViewModelActivityChangedEventArgs e);
    public class ViewModelActivityChangedEventArgs : EventArgs
    {
        public ViewModelActivityChangedEventArgs(ViewModelActivity newViewModelActivity, ViewModelActivity oldViewModelActivity)
        {
            NewViewModelActitvity = newViewModelActivity;
            OldViewModelActitvity = oldViewModelActivity;
        }
        public ViewModelActivity NewViewModelActitvity { get; private set; }
        public ViewModelActivity OldViewModelActitvity { get; private set; }
    }
}
