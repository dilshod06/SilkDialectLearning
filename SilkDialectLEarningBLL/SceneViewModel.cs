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
    public class SceneViewModel : BaseActivity
    {

        public SceneViewModel()
        {
            ViewModel.LessonSelected += (s, e) =>
            {
                NotifyPropertyChanged("Scenes");
            };
            ActivityChanged += SceneViewModel_ActivityChanged;
            EntitySelected += SceneViewModel_SceneSelected;
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
            ViewModelActivity = Activity.Learn;
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
                            ViewModel.Db.GetWithChildren<Lesson>(ViewModel.SelectedLesson.Id, true).Scenes
                        );
                    }
                    catch (Exception) { }

                }
                return scenes;
            }
        }
    }
}
