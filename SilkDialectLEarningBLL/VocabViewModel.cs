using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SilkDialectLearning.DAL;
using System.Linq;

namespace SilkDialectLearning.BLL
{
    public class VocabViewModel : BaseActivity
    {
        public VocabViewModel()
        {
            ViewModel.LessonSelected += (s, e) =>
            {
                NotifyPropertyChanged("Vocabularies");
            };

            EntitySelected += VocabViewModel_VocabularySelected;
            ActivityChanged += VocabViewModel_ActivityChanged;
        }

        async void VocabViewModel_ActivityChanged(object sender, ActivityChangedEventArgs e)
        {
            await StopPlayingAsync();
            StopHighlight();

            if (e.NewActivity == Activity.Practice)
            {
                Practice();
            }
        }

        /// <summary>
        /// When vocabulary selected it will stop any playing item and highlighting
        /// </summary>
        async void VocabViewModel_VocabularySelected(object sender, EventArgs e)
        {
            await StopPlayingAsync();
            StopHighlight();
            VocabActivity = Activity.Learn;
        }

        /// <summary>
        /// Returns the list of vocabularies under selected lesson
        /// </summary>
        public ObservableCollection<Vocabulary> Vocabularies
        {
            get
            {
                ObservableCollection<Vocabulary> vocabularies = new ObservableCollection<Vocabulary>();
                if (ViewModel.SelectedLesson != null)
                {
                    vocabularies = new ObservableCollection<Vocabulary>(ViewModel.SelectedLesson.Vocabularies);
                }
                return vocabularies;
            }
        }
    }
}
