using SilkDialectLearningDAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;

namespace SilkDialectLearningBLL
{
    public abstract class BaseActivity : INotifyPropertyChanged
    {
        #region Events

        public event ActivityChangedEventHandler ActivityChanged;

        public event HighlightItemEventHandler HighlightItem;

        public event HighlightItemEventHandler StopHighlighting;

        #endregion

        /// <summary>
        /// Contains the last highlighted item
        /// </summary>
        protected IHighlightable lastHighlighted;

        /// <summary>
        /// Contains the timer for the heghlight
        /// </summary>
        protected Timer highlightTimer;

        /// <summary>
        /// Contains the last played item
        /// </summary>
        protected IPlayable lastPlayed;

        /// <summary>
        /// Indicates whether an item is highlighting
        /// </summary>
        protected bool isHighlighting;

        public ViewModel ViewModel { get { return Global.GlobalViewModel; } }

        Activity sceneActivity;
        /// <summary>
        /// Returns the current activity
        /// </summary>
        public Activity SceneActivity
        {
            get { return sceneActivity; }
            set
            {
                var oldValue = sceneActivity;
                sceneActivity = value;
                NotifyPropertyChanged();
                var activityChanged = ActivityChanged;
                if (activityChanged != null)
                {
                    activityChanged(this, new ActivityChangedEventArgs(sceneActivity, oldValue));
                }
            }
        }
        protected async Task PlayThisItem(IPlayable sceneItem)
        {
            await StopPlaying();
            if (sceneItem != null)
            {
                lastPlayed = sceneItem;
                if (sceneItem.Phrase != null)
                    await sceneItem.Phrase.Play();
                else
                    throw new Exception("Scene Items Phrase cannot be null");
            }
            else
            {
                throw new Exception("SceneItem cannot be null");
            }
        }

        protected async Task StopPlaying()
        {
            if (lastPlayed != null)
            {
                if (lastPlayed.Phrase != null)
                    await lastPlayed.Phrase.StopPlaying();
            }
        }

        protected void HiglightThisItem(IHighlightable sceneItem, double interval)
        {
            if (sceneItem != null)
            {
                StopHighlight();
                var highlightItem = HighlightItem;
                isHighlighting = true;
                if (highlightItem != null)
                {
                    lastHighlighted = sceneItem;
                    highlightItem(this, new HighlightItemEventArgs(sceneItem));
                    Timer timer = new Timer(interval);
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        StopHighlight();
                        timer.Dispose();
                    };
                    timer.Enabled = true;
                    timer.Start();
                    highlightTimer = timer;
                }
            }
            else
            {
                throw new Exception("SceneItem cannot be null");
            }
        }

        protected void StopHighlight()
        {
            if (isHighlighting)
            {
                if (lastHighlighted != null)
                {
                    highlightTimer.Enabled = false;
                    var stopHighlighting = StopHighlighting;
                    isHighlighting = false;
                    if (stopHighlighting != null)
                    {
                        stopHighlighting(this, new HighlightItemEventArgs(lastHighlighted));
                    }
                }
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

    public class SceneViewModel : BaseActivity, INotifyPropertyChanged
    {
        public SceneViewModel()
        {
            ViewModel.LessonSelected += (s, e) =>
            {
                NotifyPropertyChanged("Scenes");
            };
            ActivityChanged += SceneViewModel_ActivityChanged;
            SceneSelected += SceneViewModel_SceneSelected;
        }

        async void SceneViewModel_SceneSelected(object sender, EventArgs e)
        {
            await StopPlaying();
            StopHighlight();
        }

        async void SceneViewModel_ActivityChanged(object sender, ActivityChangedEventArgs e)
        {
            await StopPlaying();
            StopHighlight();
        }

        #region Events
        public event EventHandler SceneSelected;

        public event EventHandler SceneItemSelected;
        #endregion

        /// <summary>
        /// Returns the list of scenes under the selected lesson
        /// </summary>
        public ObservableCollection<Scene> Scenes
        {
            get
            {
                ObservableCollection<Scene> scenes = new ObservableCollection<Scene>();
                if (ViewModel.SelectedLesson != null)
                {
                    scenes = new ObservableCollection<Scene>(ViewModel.SelectedLesson.Scenes);
                }
                return scenes;
            }
        }

        Scene selectedScene;
        /// <summary>
        /// Returns the last selected scene
        /// </summary>
        public Scene SelectedScene
        {
            get
            {
                return selectedScene;
            }
            set
            {
                selectedScene = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("SceneItems");
                var sceneSelected = SceneSelected;
                if (sceneSelected != null)
                {
                    sceneSelected(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Returns the list of SceneItems 
        /// </summary>
        public ObservableCollection<SceneItem> SceneItems
        {
            get
            {
                ObservableCollection<SceneItem> sceneItems = new ObservableCollection<SceneItem>();
                if (SelectedScene != null)
                {
                    sceneItems = SelectedScene.SceneItems;
                }
                return sceneItems;
            }

        }

        SceneItem selectedSceneItem;
        /// <summary>
        /// Returns the last selected scene item
        /// </summary>
        public SceneItem SelectedSceneItem
        {
            get { return selectedSceneItem; }
            set
            {
                selectedSceneItem = value;
                NotifyPropertyChanged();
                OnSceneItemChanged();
            }
        }

        /// <summary>
        /// This method will be fired when a SceneItem gets selected. And will fire the appropriate method based on Activity. 
        /// </summary>
        private void OnSceneItemChanged()
        {
            var sceneItemSelected = SceneItemSelected;
            if (sceneItemSelected != null)
            {
                sceneItemSelected(this, new EventArgs());
            }
            if (SceneActivity == Activity.Learn)
            {
                Learn();
            }


        }

        private void Learn()
        {
            if (SelectedSceneItem == null)
            {
                throw new Exception("SelectedSceneItem is null.");
            }
            PlayThisItem(SelectedSceneItem);
            HiglightThisItem(SelectedSceneItem, SelectedSceneItem.Phrase.SoundLength.TotalMilliseconds);
        }

        private List<SceneItem> items = new List<SceneItem>();

        public void Practice()
        {
            if (items.Count == 0)
            {
                items = Helper.MixItems<SceneItem>(SelectedScene.SceneItems.ToList());
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

    class PracticeResult
    {
        public PracticeItemStatus Status { get; set; }
    }

    public enum PracticeItemStatus
    {
        Notasked = 0,
        Asking = 1,
        Asked = 2
    }

    public delegate void HighlightItemEventHandler(object sender, HighlightItemEventArgs e);

    public class HighlightItemEventArgs : EventArgs
    {
        public HighlightItemEventArgs(IHighlightable highlightableItem)
        {
            HighlightableItem = highlightableItem;
        }
        public IHighlightable HighlightableItem { get; private set; }
    }

}
