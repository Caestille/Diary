using Diary.Core.Dtos;
using Diary.Core.ViewModels.Views;

namespace Diary.Core.Extensions
{
    internal static class DiaryDayViewModelExtensions
    {
        public static DiaryDayDto ToDto(this DiaryDayViewModel viewModel)
        {
            return new DiaryDayDto() { Name = viewModel.Name, Notes = viewModel.Notes, Entries = viewModel.ChildViewModels.Select(x => (x as DiaryEntryViewModel).ToDto()).ToList(), DayOfWeek = viewModel.DayOfWeek };
        }
    }
}
