using Diary.ViewModels;
using System.Collections.Generic;
using System.Windows;
using Diary.Core.ViewModels.Base;
using CoreUtilities.Services.RegistryInteraction;
using Diary.Core.ViewModels.Views;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Diary.Core.Dtos;

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

        protected override void OnStartup(StartupEventArgs e)
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

            var weeks = Directory.GetFiles(WorkingDirectory)
                .Where(x => Guid.TryParse(Path.GetFileNameWithoutExtension(x), out _));
            var weekVms = weeks.Select(
                x => DiaryWeekViewModel.FromDto(
                    JsonSerializer.Deserialize<DiaryWeekDto>(File.ReadAllText(x)),
                    WorkingDirectory,
                    Guid.Parse(Path.GetFileNameWithoutExtension(x)))).ToList();

            var viewModels = new List<ViewModelBase>()
            {
                new CalendarViewModel(WorkingDirectory, weekVms),
                taggingVm,
            };

            var mainViewModel = new MainViewModel(viewModels, appRegistryService);
            viewModels[0].SelectCommand.Execute(null);

            var mainView = new MainWindow()
            {
                DataContext = mainViewModel
            };

            mainView.Show();
        }
    }
}