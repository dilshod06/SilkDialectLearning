using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SilkDialectLearning.BLL.Timers;
using SilkDialectLearning.DAL;
using SQLiteNetExtensions.Extensions;

namespace SilkDialectLearning.BLL
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

        Activity sceneActivity = Activity.Learn;
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
                    await ViewModel.AudioManager.Play(sceneItem.Phrase);
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
                    await ViewModel.AudioManager.StopPlaying();
            }
        }

        /// <summary>
        /// Stops the any highlighting item and starts highlighting current item
        /// </summary>
        /// <param name="sceneItem">Item to highlight</param>
        /// <param name="interval">Higligh for X milliseconds</param>
        protected void HiglightThisItem(IHighlightable sceneItem, double interval, PracticeItemResult itemResult = PracticeItemResult.Default)
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
                    highlightItem(this, new HighlightItemEventArgs(sceneItem, itemResult));
                    //Timer will automatically stop the highlighting item after specific time and disposes itself.
                    Timer timer = new Timer(interval);
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        StopHighlight();
                    };
                    timer.Enabled = true;
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
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }

    public class SceneViewModel : BaseActivity
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

            if (e.NewActivity == Activity.Practice)
            {
                Practice();
            }
        }

        #region Events
        public event EventHandler SceneSelected;

        public event EventHandler SceneItemSelected;

        public event PraceticeFinishedEventHandler PracticeFinished;

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
                    try
                    {
                        scenes = new ObservableCollection<Scene>
                        (
                            ViewModel.Db.GetWithChildren<Lesson>(ViewModel.SelectedLesson.Id, true).Scenes.OrderBy(s => s.Name)
                        );
                    }
                    catch (Exception) { }

                }
                return scenes;
            }
        }

        private Scene selectedScene;
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
                SceneActivity = Activity.Learn;
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
                    try
                    {
                        sceneItems = new ObservableCollection<SceneItem>
                        (
                            ViewModel.Db.GetWithChildren<Scene>(SelectedScene.Id).SceneItems
                        );
                    }
                    catch (Exception) { }
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
            try
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
                else if (SceneActivity == Activity.Practice)
                {
                    ContinuePractice();
                }
            }
            catch (Exception ex)
            {   
            }
            

        }

        /// <summary>
        /// Plays the selected item
        /// </summary>
        private async void Learn()
        {
            if (SelectedSceneItem == null)
            {
                throw new Exception("SelectedSceneItem is null.");
            }
            await PlayThisItemAsync(SelectedSceneItem);
            HiglightThisItem(SelectedSceneItem, SelectedSceneItem.Phrase.SoundLength.TotalMilliseconds);
        }

        private List<PracticeResult<SceneItem>> itemsForPractice = new List<PracticeResult<SceneItem>>();

        private PracticeResult<SceneItem> lastPlayedItem;

        /// <summary>
        /// Prepares the items for practice.
        /// </summary>
        public async void Practice()
        {
            itemsForPractice = Helper.MixItems<PracticeResult<SceneItem>>(SelectedScene.SceneItems.Select(i => new PracticeResult<SceneItem>(i)).ToList());
            lastPlayedItem = itemsForPractice.ElementAt(0);
            lastPlayedItem.Status = PracticeItemStatus.Asking;
            await PlayThisItemAsync(lastPlayedItem.Item);
            lastPlayedItem.Status = PracticeItemStatus.Asked;
        }

        public async void RepeatLastPlayedItem()
        {
            await PlayThisItemAsync(lastPlayedItem.Item);
        }

        private async void ContinuePractice()
        {
            if (SelectedSceneItem != lastPlayedItem.Item)
            {
                if (lastPlayedItem.WrongAnswersCount == 3)
                {
                    lastPlayedItem.WrongAnswersCount = 0;
                    await PlayThisItemAsync(lastPlayedItem.Item);
                    HiglightThisItem(lastPlayedItem.Item, lastPlayedItem.Item.Phrase.SoundLength.TotalMilliseconds, PracticeItemResult.Fixed);
                    return;
                }
                lastPlayedItem.WrongAnswersCount++;
                await PlayThisItemAsync(lastPlayedItem.Item);
                HiglightThisItem(SelectedSceneItem, lastPlayedItem.Item.Phrase.SoundLength.TotalMilliseconds, PracticeItemResult.Wrong);
            }
            else
            {
                await PlayThisItemAsync(lastPlayedItem.Item);
                HiglightThisItem(lastPlayedItem.Item, lastPlayedItem.Item.Phrase.SoundLength.TotalMilliseconds, PracticeItemResult.Right);

                Action playNextAction = new Action(async () =>
                {
                    lastPlayedItem = itemsForPractice.FirstOrDefault(i => i.Status == PracticeItemStatus.Notasked);

                    if (lastPlayedItem == null)
                    {
                        var practiceFinished = PracticeFinished;
                        if (practiceFinished != null)
                        {
                            practiceFinished(this, new PraceticeFinishedEventArgs("Do you want redo this scene?"));
                        }
                        return;
                    }
                    lastPlayedItem.Status = PracticeItemStatus.Asking;
                    await PlayThisItemAsync(lastPlayedItem.Item);
                    lastPlayedItem.Status = PracticeItemStatus.Asked;
                });
                RunAction(playNextAction, lastPlayedItem.Item.Phrase.SoundLength.TotalMilliseconds);
            }
        }

        private void RunAction(Action action, double runAfter)
        {
            if (runAfter < 0) runAfter = 0;
            Timer timer = new Timer(runAfter);
            timer.Elapsed += (s, e) =>
            {
                timer.Stop();
                action.Invoke();
            };
            timer.Enabled = true;
        }
    }

    public class PracticeResult<T> where T : IEntity
    {
        public PracticeResult(T item)
        {
            Item = item;
            Status = PracticeItemStatus.Notasked;
        }

        public T Item { get; private set; }

        public PracticeItemStatus Status { get; set; }

        public int WrongAnswersCount { get; set; }
    }

    public enum PracticeItemStatus
    {
        Notasked = 0,
        Asking = 1,
        Asked = 2
    }

    public enum PracticeItemResult
    {
        Default = 0,
        Wrong = 1,
        Right = 2,
        Fixed = 4
    }

    public delegate void HighlightItemEventHandler(object sender, HighlightItemEventArgs e);

    public class HighlightItemEventArgs : EventArgs
    {
        public HighlightItemEventArgs(IHighlightable highlightableItem, PracticeItemResult practiceItemResult = PracticeItemResult.Default)
        {
            HighlightableItem = highlightableItem;
            PracticeItemResult = practiceItemResult;
        }
        public IHighlightable HighlightableItem { get; private set; }

        public PracticeItemResult PracticeItemResult { get; private set; }
    }

    public delegate void PraceticeFinishedEventHandler(object sender, PraceticeFinishedEventArgs e);

    public class PraceticeFinishedEventArgs : EventArgs
    {
        public string Message { get; set; }

        public PraceticeFinishedEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
