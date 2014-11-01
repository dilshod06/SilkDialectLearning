using SilkDialectLearningDAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SilkDialectLEarningBLL
{
    public class SceneViewModel : INotifyPropertyChanged
    {
        #region Events
        public event EventHandler SceneSelected;

        public event EventHandler SceneItemSelected;

        public event ActivityChangedEventHandler ActivityChanged;

        public event HighlightItemEventHandler HighlightItem;
        #endregion

        private SceneItem lastHighlighted;
        private SceneItem lastPlayed;

        public ViewModel ViewModel { get { return Global.GlobalViewModel; } }

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
            Timer timer = new Timer(SelectedSceneItem.Phrase.SoundLength.TotalMilliseconds);
            timer.Elapsed += (s, e) => { 
                
            };
        }

        private void PlayThisItem(SceneItem sceneItem)
        {
            if (sceneItem != null)
            {
                lastPlayed = sceneItem;
                sceneItem.Phrase.Play();
            }
            else
            {
                throw new Exception("SceneItem cannot be null");
            }
        }

        private void HiglightThisItem(SceneItem sceneItem)
        {
            if (sceneItem != null)
            {
                var highlightItem = HighlightItem;
                if (highlightItem != null)
                {
                    lastHighlighted = sceneItem;
                    highlightItem(this, new HighlightItemEventArgs(sceneItem));
                }
            }
            else
            {
                throw new Exception("SceneItem cannot be null");
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


    public delegate void HighlightItemEventHandler(SceneViewModel sender, HighlightItemEventArgs e);

    public class HighlightItemEventArgs : EventArgs
    {
        public HighlightItemEventArgs(SceneItem sceneItem)
        {
            SceneItem = sceneItem;
        }
        public SceneItem SceneItem { get; private set; }
    }
    
}
