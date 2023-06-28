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
using SplashScreen = Diary.Views.SplashScreen;
using System.Threading.Tasks;

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

            var splashVm = new SplashScreenViewModel()
            {
                ImageUri = new Uri("ms-appx:///Resources/Icon.ico"),
                ApplicationName = "Diary",
                CopyrightNotice = $"© {DateTime.Now.Year} Mercedes AMG F1. All rights reserved"
            };
            var splashScreen = new SplashScreen() { DataContext = splashVm };
            splashVm.UpdateStep = "Intialising";
            splashScreen.Show();

            if (!File.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }

            var appRegistryService = new RegistryService(@"SOFTWARE\Diary");

            var taggingVm = new DataTaggingViewModel(WorkingDirectory);

            var viewModels = await Task.Run(() => 
            {
                double i = 0;
                var weeks = Directory.GetFiles(WorkingDirectory)
                    .Where(x => Guid.TryParse(Path.GetFileNameWithoutExtension(x), out _))
                    .ToList();
                double count = weeks.Count();
                var weekVms = new List<DiaryWeekViewModel>();
                foreach (var week in weeks)
                {
                    i++;
                    splashVm.ProgressPercent = (i / count) * 100;
                    weekVms.Add(DiaryWeekViewModel.FromDto(
                        JsonSerializer.Deserialize<DiaryWeekDto>(File.ReadAllText(week)),
                        WorkingDirectory,
                        Guid.Parse(Path.GetFileNameWithoutExtension(week))));
                }
                var vms = new List<ViewModelBase>()
                {
                    new TakeMeToTodayViewModel(),
                    new CalendarViewModel(WorkingDirectory, weekVms),
                    taggingVm,
                };
                return vms;
            });

            var mainViewModel = new MainViewModel(viewModels, appRegistryService);
            viewModels[0].SelectCommand.Execute(null);

            var mainView = new MainWindow()
            {
                DataContext = mainViewModel
            };

            mainView.Show();
            splashScreen.Close();
        }
    }
}