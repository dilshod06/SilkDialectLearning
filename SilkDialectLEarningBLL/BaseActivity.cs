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

        public event EventHandler EntitySelected;

        public event EventHandler EntityItemSelected;

        public event PraceticeFinishedEventHandler PracticeFinished;

        #endregion

        private IEntity selectedEntity;
        /// <summary>
        /// Returns the last selected ViewModelEntity. If ViewModel entity is A Scene then returns last selected scene.
        /// If ViewModelEntity is a Vocabulary then returns last selected Vocabulary
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

        IEntity selectedEntityItem;
        /// <summary>
        /// Returns the last selected ViewModelEntityItem. If ViewModel entity is A Scene then returns last selected sceneItem.
        /// If ViewModelEntity is a Vocabulary then returns last selected Word
        /// </summary>
        public IEntity SelectedEntityItem
        {
            get { return selectedEntityItem; }
            set
            {
                selectedEntityItem = value;
                NotifyPropertyChanged();
                OnViewModelEntityItemChanged();
            }
        }

        /// <summary>
        /// This method will be fired when a ViewmodelEntityItems change. And will fire the appropriate method based on Activity. 
        /// </summary>
        private void OnViewModelEntityItemChanged()
        {
            try
            {
                var sceneItemSelected = EntityItemSelected;
                if (sceneItemSelected != null)
                {
                    sceneItemSelected(this, new EventArgs());
                }
                if (ViewModelActivity == Activity.Learn)
                {
                    Learn();
                }
                else if (ViewModelActivity == Activity.Practice)
                {
                    ContinuePractice();
                }
            }
            catch (Exception ex)
            {
            }


        }

        public async void RepeatLastPlayedItem()
        {
            await PlayThisItemAsync(lastPlayedItem.Item);
        }
        /// <summary>
        /// Plays the selected ViewModelEntityItem. If ViewModel entity is A Scene then plays selected scene.
        /// If ViewModelEntity is a Vocabulary then plays selected Word
        /// </summary>
        private async void Learn()
        {
            if (SelectedEntityItem == null)
            {
                throw new Exception("SelectedViewModelEntityItem is null.");
            }
            await PlayThisItemAsync(SelectedEntityItem as IPlayable);
            double a = 2;
            HiglightThisItem(SelectedEntityItem as IHighlightable, a);
        }

        private List<PracticeResult> itemsForPractice = new List<PracticeResult>();


        private PracticeResult lastPlayedItem;

        /// <summary>
        /// Prepares the ViewModelEntityItems for practice.
        /// </summary>
        public async void Practice()
        {
            Scene scene = SelectedEntity as Scene;
            if (scene != null)
            {
                itemsForPractice = Helper.MixItems<PracticeResult>((SelectedEntity as Scene).SceneItems.Select(i => new PracticeResult(i)).ToList());                            }
            else
            {
                Vocabulary vocab = SelectedEntity as Vocabulary;
                if (vocab != null)
                {
                    itemsForPractice = Helper.MixItems<PracticeResult>((SelectedEntity as Vocabulary).Words.Select(i => new PracticeResult(i)).ToList());
                }
            }

            if (itemsForPractice != null)
            {
                lastPlayedItem = itemsForPractice.FirstOrDefault();
                lastPlayedItem.Status = PracticeItemStatus.Asking;
                await PlayThisItemAsync(lastPlayedItem.Item);
                lastPlayedItem.Status = PracticeItemStatus.Asked;
            }
            else
            {
                throw new Exception("ItemsForPractice should not be a null");
            }

        }
        /// <summary>
        /// Contains the last highlighted ViewModelEntityItem
        /// </summary>
        protected IHighlightable lastHighlighted;

        /// <summary>
        /// Contains the timer for the heghlight
        /// </summary>
        protected Timer highlightTimer;

        /// <summary>
        /// Contains the last played ViewModelEntity item
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

        Activity viewModelActivity = Activity.Learn;
        /// <summary>
        /// Returns the current viewModelActivity
        /// </summary>
        public Activity ViewModelActivity
        {
            get { return viewModelActivity; }
            set
            {
                var oldValue = viewModelActivity;
                viewModelActivity = value;
                NotifyPropertyChanged();
                var activityChanged = ActivityChanged;
                if (activityChanged != null)
                {
                    activityChanged(this, new ActivityChangedEventArgs(viewModelActivity, oldValue));
                }
            }
        }

        /// <summary>
        /// Stops the if there is anything that's playing and starts playing current ViewModelEntityItem.
        /// </summary>
        /// <param name="item">Item to play</param>
        /// <returns></returns>
        protected async Task PlayThisItemAsync(IPlayable item)
        {
            await StopPlayingAsync();
            if (item != null)
            {
                //Sets the lastPlayed item so that we play again or helpfull if need to stop it before it finished playing
                lastPlayed = item;
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
        /// Stops the the current playing ViewModelEntityItem
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
        /// Stops the any highlighting item and starts highlighting current ViewModelEntityItem
        /// </summary>
        /// <param name="EntityItem">Item to highlight</param>
        /// <param name="interval">Higligh for X milliseconds</param>
        protected void HiglightThisItem(IHighlightable EntityItem, double interval, PracticeItemResult itemResult = PracticeItemResult.Default)
        {
            if (EntityItem != null)
            {
                StopHighlight();
                var highlightItem = HighlightItem;

                //Setting the isHiglighting to true to help Stop highlighting
                isHighlighting = true;
                if (highlightItem != null)
                {
                    lastHighlighted = EntityItem;
                    highlightItem(this, new HighlightItemEventArgs(EntityItem, itemResult));
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
        /// Stops the current highlighting ViewModelEntityItem
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
