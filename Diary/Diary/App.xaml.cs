using Diary.ViewModels;
using System.Collections.Generic;
using System.Windows;
using Diary.Core.ViewModels.Base;
using CoreUtilities.Services.RegistryInteraction;
using Diary.Core.ViewModels.Views;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Diary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static string[]? StartingArgs { get; set; }

        public static bool MenuStartOpen { get; }

        public static string WorkingDirectory
            => @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Diary";

        public App() { }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                StartingArgs = e.Args;
            }

            if (!File.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }

            var appRegistryService = new RegistryService(@"SOFTWARE\Diary");

            var taggingVm = new DataTaggingViewModel(WorkingDirectory);
            var viewModels = new List<ViewModelBase>()
            {
                new DiaryWeekCollectionViewModel(WorkingDirectory),
                taggingVm,
            };

            var mainViewModel = new MainViewModel(viewModels, appRegistryService);
            viewModels[0].SelectCommand.Execute(null);

            var mainView = new MainWindow()
            {
                DataContext = mainViewModel
            };

            await Task.Delay(1000);

            mainView.Show();
        }
    }
}