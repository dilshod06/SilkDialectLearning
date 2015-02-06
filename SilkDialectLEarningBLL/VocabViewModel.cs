using SilkDialectLearningDAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SilkDialectLearningBLL
{
    public class VocabViewModel : BaseActivity, INotifyPropertyChanged
    {
        public VocabViewModel()
        {
            ViewModel.LessonSelected += (s, e) =>
            {
                NotifyPropertyChanged("Vocabularies");
            };

            VocabularySelected += VocabViewModel_VocabularySelected;
        }

        /// <summary>
        /// When vocabulary selected it will stop any playing item and highlighting
        /// </summary>
        async void VocabViewModel_VocabularySelected(object sender, EventArgs e)
        {
            await StopPlayingAsync();
            StopHighlight();
        }

        private List<PracticeResult<Vocabulary>> vocabulariesForPractice = new List<PracticeResult<Vocabulary>>();

        public void Practice()
        {

        }
        public event EventHandler VocabularySelected;

        public event EventHandler WordSelected;

        /// <summary>
        /// Returns the instance of ViewModel
        /// </summary>
        public ViewModel ViewModel
        {
            get
            {
                return Global.GlobalViewModel;
            }
        }

        /// <summary>
        /// Returns the list of Vocabularies under selected lesson
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

        Vocabulary selectedVocabulary;
        /// <summary>
        /// Returns the last selected vocabulary
        /// </summary>
        public Vocabulary SelectedVocabulary
        {
            get
            {
                return selectedVocabulary;
            }
            set
            {
                selectedVocabulary = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Returns the list of words under selected vocabulary
        /// </summary>
        public ObservableCollection<Word> Words
        {
            get
            {
                ObservableCollection<Word> words = new ObservableCollection<Word>();
                if (SelectedVocabulary != null)
                {
                    words = new ObservableCollection<Word>(SelectedVocabulary.Words);
                }
                return words;
            }
        }

        Word selectedWord;
        /// <summary>
        /// Returns the last selected word
        /// </summary>
        public Word SelectedWord
        {
            get
            {
                return selectedWord;
            }
            set
            {
                selectedWord = value;
                NotifyPropertyChanged();
                OnVocabWordChanged();
            }
        }

        private void OnVocabWordChanged()
        {
            var sceneItemSelected = WordSelected;
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
                //ContinuePractice();
            }

        }

        private void Learn()
        {
            if (selectedWord == null)
            {
                throw new Exception("Selected word is null.");
            }
            PlayThisItemAsync(SelectedWord);
            HiglightThisItem(SelectedWord, SelectedWord.Phrase.SoundLength.TotalMilliseconds);
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
}
