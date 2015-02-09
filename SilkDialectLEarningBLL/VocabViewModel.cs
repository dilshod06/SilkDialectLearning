using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SilkDialectLearning.DAL;

namespace SilkDialectLearning.BLL
{
    public class VocabViewModel : BaseActivity, INotifyPropertyChanged
    {
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
}
