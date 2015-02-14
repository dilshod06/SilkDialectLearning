using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SilkDialectLearning.BLL.Timers;
using SilkDialectLearning.DAL;

namespace SilkDialectLearning.BLL
{
    public abstract class BaseActivity : INotifyPropertyChanged
    {
        #region Events

        public event ActivityChangedEventHandler ActivityChanged;

        public event HighlightItemEventHandler HighlightItem;

        public event HighlightItemEventHandler StopHighlighting;

        public event EventHandler EntitySelected;

        public event EventHandler EntityItemSelected;

        public event PraceticeFinishedEventHandler PracticeFinished;

        #endregion

        #region Properties and Fields

        private IEntity selectedEntity;
        /// <summary>
        /// Returns the last selected scene
        /// </summary>
        public IEntity SelectedEntity
        {
            get
            {
                return selectedEntity;
            }
            set
            {
                selectedEntity = value;
                NotifyPropertyChanged();
                var sceneSelected = EntitySelected;
                if (sceneSelected != null)
                {
                    sceneSelected(this, new EventArgs());
                }
            }
        }

        private IEntity selectedEntityItem;
        /// <summary>
        /// Returns the last selected scene item
        /// </summary>
        public IEntity SelectedEntityItem // It should be SceneItem or Word
        {
            get { return selectedEntityItem; }
            set
            {
                selectedEntityItem = value;
                NotifyPropertyChanged();
                OnSceneItemChanged();
            }
        }

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
        
        Activity vocabActivity = Activity.Learn;
        /// <summary>
        /// Returns the current vocabulary activity
        /// </summary>
        public Activity VocabActivity
        {
            get { return vocabActivity; }
            set
            {
                var oldValue = vocabActivity;
                vocabActivity = value;
                NotifyPropertyChanged();
                var activityChanged = ActivityChanged;
                if (activityChanged != null)
                {
                    activityChanged(this, new ActivityChangedEventArgs(vocabActivity, oldValue));
                }
            }
        }

        private List<PracticeResult> itemsForPractice = new List<PracticeResult>();

        /// <summary>
        /// Contains Last Played Item for only practice activity
        /// </summary>
        private PracticeResult lastPlayedItem;

        /// <summary>
        /// Contains the last highlighted item
        /// </summary>
        protected IHighlightable LastHighlighted;

        /// <summary>
        /// Contains the timer for the heghlight
        /// </summary>
        protected Timer HighlightTimer;

        /// <summary>
        /// Contains the last played item
        /// </summary>
        protected IPlayable LastPlayed;

        /// <summary>
        /// Indicates whether an item is highlighting
        /// </summary>
        protected bool IsHighlighting;

        #endregion

        #region Methods

        /// <summary>
        /// This method will be fired when a SceneItem gets selected. And will fire the appropriate method based on Activity. 
        /// </summary>
        private void OnSceneItemChanged()
        {
            try
            {
                var sceneItemSelected = EntityItemSelected;
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
                Debug.WriteLine(ex.InnerException);
            }


        }

        /// <summary>
        /// Stops the if there is anything that's playing and starts playing current item.
        /// </summary>
        /// <param name="item">Item to play</param>
        /// <returns></returns>
        protected async Task PlayThisItemAsync(IPlayable item)
        {
            await StopPlayingAsync();
            if (item != null)
            {
                //Sets the lastPlayed item so that we play again or helpfull if need to stop it before it finished playing
                LastPlayed = item;
                if (item.Phrase != null)
                    await ViewModel.AudioManager.Play(item.Phrase);
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
            if (LastPlayed != null)
            {
                if (LastPlayed.Phrase != null)
                    await ViewModel.AudioManager.StopPlaying();
            }
        }

        /// <summary>
        /// Stops the any highlighting item and starts highlighting current item
        /// </summary>
        /// <param name="sceneItem">Item to highlight</param>
        /// <param name="interval">Higlight for X milliseconds</param>
        /// <param name="itemResult">Result for practice highlighting</param>
        protected void HiglightThisItem(IHighlightable sceneItem, double interval, PracticeItemResult itemResult = PracticeItemResult.Default)
        {
            if (sceneItem != null)
            {
                StopHighlight();
                var highlightItem = HighlightItem;

                //Setting the isHiglighting to true to help Stop highlighting
                IsHighlighting = true;
                if (highlightItem != null)
                {
                    LastHighlighted = sceneItem;
                    highlightItem(this, new HighlightItemEventArgs(sceneItem, itemResult));
                    //Timer will automatically stop the highlighting item after specific time and disposes itself.
                    Timer timer = new Timer(interval);
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        StopHighlight();
                    };
                    timer.Enabled = true;
                    HighlightTimer = timer;
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
            if (IsHighlighting)
            {
                if (LastHighlighted != null)
                {
                    //highlightingTimer is created by HighlightThisItem() method. So we are just disabling the timer and disposing.
                    HighlightTimer.Enabled = false;
                    HighlightTimer.Dispose();
                    var stopHighlighting = StopHighlighting;
                    IsHighlighting = false;
                    if (stopHighlighting != null)
                    {
                        stopHighlighting(this, new HighlightItemEventArgs(LastHighlighted));
                    }
                }
            }
        }

        /// <summary>
        /// Will be call to repeat last played item
        /// </summary>
        public async void RepeatLastPlayedItem()
        {
            await PlayThisItemAsync(lastPlayedItem.Item);
        }
        
        /// <summary>
        /// Plays the selected item
        /// </summary>
        private async void Learn()
        {
            if (SelectedEntityItem == null)
            {
                throw new Exception("SelectedEntityItem is null.");
            }
            IPlayable playableItem = SelectedEntityItem as IPlayable;
            if (playableItem != null)
            {
                await PlayThisItemAsync(playableItem);
                HiglightThisItem(SelectedEntityItem as IHighlightable, playableItem.Phrase.SoundLength.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Prepares the items for practice.
        /// </summary>
        public async void Practice()
        {
            if (SelectedEntity is Scene)
                itemsForPractice = Helper.MixItems<PracticeResult>((SelectedEntity as Scene).SceneItems.Select(i => new PracticeResult(i)).ToList());
            else if(SelectedEntity is Vocabulary)
                itemsForPractice = Helper.MixItems<PracticeResult>((SelectedEntity as Vocabulary).Words.Select(i => new PracticeResult(i)).ToList());

            lastPlayedItem = itemsForPractice.FirstOrDefault();
            lastPlayedItem.Status = PracticeItemStatus.Asking;
            await PlayThisItemAsync(lastPlayedItem.Item);
            lastPlayedItem.Status = PracticeItemStatus.Asked;
        }

        /// <summary>
        /// When user select some item it will be compare last played item if its equal play's next random item else repeat last played item  
        /// </summary>
        protected async void ContinuePractice()
        {
            if (SelectedEntityItem != lastPlayedItem.Item)
            {
                if (lastPlayedItem.WrongAnswersCount == 3)
                {
                    lastPlayedItem.WrongAnswersCount = 0;
                    await PlayThisItemAsync(lastPlayedItem.Item);
                    HiglightThisItem((lastPlayedItem.Item as IHighlightable), lastPlayedItem.Item.Phrase.SoundLength.TotalMilliseconds, PracticeItemResult.Fixed);
                    return;
                }
                lastPlayedItem.WrongAnswersCount++;
                await PlayThisItemAsync(lastPlayedItem.Item);
                HiglightThisItem(SelectedEntityItem as IHighlightable, lastPlayedItem.Item.Phrase.SoundLength.TotalMilliseconds, PracticeItemResult.Wrong);
            }
            else
            {
                await PlayThisItemAsync(lastPlayedItem.Item);
                HiglightThisItem((lastPlayedItem.Item as IHighlightable), lastPlayedItem.Item.Phrase.SoundLength.TotalMilliseconds, PracticeItemResult.Right);

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

        protected void RunAction(Action action, double runAfter)
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

        #endregion

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
}
