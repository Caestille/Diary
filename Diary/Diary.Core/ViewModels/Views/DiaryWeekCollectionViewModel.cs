using Diary.Core.Dtos;
using Diary.Core.Messages;
using Diary.Core.ViewModels.Base;
using System.Security.Cryptography.Pkcs;
using System.Text.Json;
using Microsoft.Toolkit.Mvvm.Messaging;
using CoreUtilities.HelperClasses;

namespace Diary.Core.ViewModels.Views
{
    public class DiaryWeekCollectionViewModel : ViewModelBase
    {
        public DiaryWeekCollectionViewModel(string workingDirectory) : base("", () => new DiaryWeekViewModel(workingDirectory))
        {
            var weeks = Directory.GetFiles(workingDirectory).Where(x => Guid.TryParse(Path.GetFileNameWithoutExtension(x), out _));
            ChildViewModels.AddRange(weeks.Select(x => DiaryWeekViewModel.FromDto(JsonSerializer.Deserialize<DiaryWeekDto>(File.ReadAllText(x)), workingDirectory, Guid.Parse(Path.GetFileNameWithoutExtension(x)))).OrderByDescending(x => x.WeekStart));
        }

        protected override void BindMessages()
        {
            Messenger.Register<WeekChangedMessage>(this, (recipient, sender) => 
            {
                var vms = new List<ViewModelBase>(ChildViewModels);
                ChildViewModels = new RangeObservableCollection<ViewModelBase>(vms.OrderByDescending(x => (x as DiaryWeekViewModel).WeekStart));
            });
            base.BindMessages();
        }

        public override void AddChild(ViewModelBase viewModelToAdd = null, string name = "", int? index = null)
        {
            base.AddChild(viewModelToAdd, name, 0);
        }
    }
}
