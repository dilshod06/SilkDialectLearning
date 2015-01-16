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
        
        /// <summary>
        /// Returns the instance of global ViewModel.
        /// </summary>
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
        
        /// <summary>
        /// Stops the if there is anything that's playing and starts playing current item.
        /// </summary>
        /// <param name="sceneItem">Item to play</param>
        /// <returns></returns>
        protected async Task PlayThisItemAsync(IPlayable sceneItem)
        {
            await StopPlayingAsync();
            if (sceneItem != null)
            {
                //Sets the lastPlayed item so that we play again or helpfull if need to stop it before it finished playing
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

        /// <summary>
        /// Stops the the current playing item
        /// </summary>
        /// <returns></returns>
        protected async Task StopPlayingAsync()
        {
            if (lastPlayed != null)
            {
                if (lastPlayed.Phrase != null)
                    await lastPlayed.Phrase.StopPlaying();
            }
        }

        /// <summary>
        /// Stops the any highlighting item and starts highlighting current item
        /// </summary>
        /// <param name="sceneItem">Item to highlight</param>
        /// <param name="interval">Higligh for X milliseconds</param>
        protected void HiglightThisItem(IHighlightable sceneItem, double interval)
        {
            if (sceneItem != null)
            {
                StopHighlight();
                var highlightItem = HighlightItem;
                //Setting the isHiglighting to true to help Stop highlighting
                isHighlighting = true;
                if (highlightItem != null)
                {
                    lastHighlighted = sceneItem;
                    highlightItem(this, new HighlightItemEventArgs(sceneItem));
                    //Timer will automatically stop the highlighting item after specific time and disposes itself.
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

        /// <summary>
        /// Stops the current highlighting item
        /// </summary>
        protected void StopHighlight()
        {
            if (isHighlighting)
            {
                if (lastHighlighted != null)
                {
                    //highlightingTimer is created by HighlightThisItem() method. So we are just disabling the timer and disposing.
                    highlightTimer.Enabled = false;
                    highlightTimer.Dispose();
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
            this.PropertyChanged += SceneViewModel_PropertyChanged;
        }

        void SceneViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Levels")
            {
                
            }
        }
        
        /// <summary>
        /// When scene selected it will stop any playing item and highlighting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void SceneViewModel_SceneSelected(object sender, EventArgs e)
        {
            await StopPlayingAsync();
            StopHighlight();
        }

        /// <summary>
        /// When activity changes it will stop any playing item and highlighting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void SceneViewModel_ActivityChanged(object sender, ActivityChangedEventArgs e)
        {
            await StopPlayingAsync();
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

        /// <summary>
        /// Plays the selected item
        /// </summary>
        private void Learn()
        {
            if (SelectedSceneItem == null)
            {
                throw new Exception("SelectedSceneItem is null.");
            }
            PlayThisItemAsync(SelectedSceneItem);
            HiglightThisItem(SelectedSceneItem, SelectedSceneItem.Phrase.SoundLength.TotalMilliseconds);
        }

        private List<SceneItem> itemsForPractice = new List<SceneItem>();

        /// <summary>
        /// Prepares the items for practice.
        /// </summary>
        public void Practice()
        {
            if (itemsForPractice.Count == 0)
            {
                itemsForPractice = Helper.MixItems<SceneItem>(SelectedScene.SceneItems.ToList());
            }
        }

        #region Notify

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    public class PracticeResult
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
