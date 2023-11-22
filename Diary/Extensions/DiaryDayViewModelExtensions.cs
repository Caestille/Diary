using Diary.Dtos;
using Diary.ViewModels.Views;

namespace Diary.Extensions
{
    internal static class DiaryDayViewModelExtensions
    {
        public static DiaryDayDto ToDto(this DiaryDayViewModel viewModel)
        {
            return new DiaryDayDto() { Name = viewModel.Name, Notes = viewModel.Notes, Entries = viewModel.ChildViewModels.Select(x => (x as DiaryEntryViewModel).ToDto()).ToList(), DayOfWeek = viewModel.DayOfWeek };
        }
    }
}
