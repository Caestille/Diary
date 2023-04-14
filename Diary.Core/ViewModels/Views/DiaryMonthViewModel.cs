using Diary.Core.Messages;
using Diary.Core.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.Messaging;
using CoreUtilities.HelperClasses;
using System.Globalization;

namespace Diary.Core.ViewModels.Views
{
    public class DiaryMonthViewModel : ViewModelBase
    {
        private string shortName;
        public string ShortName
        {
            get => shortName;
            set => SetProperty(ref shortName, value);
        }

        public DiaryMonthViewModel(
            string workingDirectory, IEnumerable<DiaryWeekViewModel> startingWeeks = null, string startingName = "")
            : base("", () => new DiaryWeekViewModel(workingDirectory))
        {
            SupportsDeleting = true;

            if (!string.IsNullOrEmpty(startingName))
            {
                Name = startingName;
                ShortName = DateTime.ParseExact(Name, "MMMM", CultureInfo.InvariantCulture).ToString("MMM");
            }
            else
            {
                Name = DateTime.Now.ToString("MMMM");
                ShortName = DateTime.Now.ToString("MMM");
            }

            if (startingWeeks != null && startingWeeks.Any())
            {
                var weeks = startingWeeks
                    .Where(x => x.WeekStart.Month == DateTime.ParseExact(Name, "MMMM", CultureInfo.InvariantCulture).Month)
                    .OrderBy(x => x.WeekStart);
                foreach (var week in weeks)
                {
                    AddChild(week);
                }
            }
            else
            {
                AddChild();
                IsShowingChildren = true;
                ChildViewModels.Last().SelectCommand.Execute(this);
            }
        }

        protected override void BindMessages()
        {
            Messenger.Register<WeekChangedMessage>(this, (recipient, sender) => 
            {
                var vms = new List<ViewModelBase>(ChildViewModels);
                ChildViewModels = new RangeObservableCollection<ViewModelBase>(
                    vms.OrderByDescending(x => (x as DiaryWeekViewModel).WeekStart));
            });
            base.BindMessages();
        }

        public override void AddChild(ViewModelBase viewModelToAdd = null, string name = "", int? index = null)
        {
            base.AddChild(viewModelToAdd, name, 0);
        }
    }
}
