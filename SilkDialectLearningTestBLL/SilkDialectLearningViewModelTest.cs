using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SilkDialectLearningBLL;
using System.Linq;
using System.Threading.Tasks;


namespace SilkDialectLearningTestBLL
{
    [TestClass]
    public class SilkDialectLearningViewModelTest
    {
        [TestInitialize]
        public void SetUp()
        {
            InitializeDatabaseFile();
        }

        [TestMethod]
        public void DeleteEntityTest()
        {

            ViewModel.SelectedLanguage = ViewModel.Languages.FirstOrDefault();
            ViewModel.SelectedLevel = ViewModel.SelectedLanguage.Levels.FirstOrDefault();
            ViewModel.SelectedUnit = ViewModel.SelectedLevel.Units.FirstOrDefault();
            ViewModel.SelectedLesson = ViewModel.SelectedUnit.Lessons.FirstOrDefault();
            var startTime = DateTime.Now;
            Task.Run(async () =>
            {
                await ViewModel.Delete(ViewModel.SelectedLanguage);
            }).GetAwaiter().GetResult();

            var endTime = DateTime.Now;

            var a = endTime - startTime;

        }

        private void InitializeDatabaseFile()
        {
            DirectoryInfo currentDir = new DirectoryInfo(".\\");
            if (currentDir.GetFiles().All(f => f.Name != "SilkDialectLearning.db"))
            {
                Console.WriteLine("Database file not found! We will create one for you");
                Global.CreateDatabase = true;
            }
            Global.DatabasePath = currentDir.FullName + "SilkDialectLearning.db";

            ViewModel = Global.GlobalViewModel;
        }

        public ViewModel ViewModel { get; private set; }

    }
}
