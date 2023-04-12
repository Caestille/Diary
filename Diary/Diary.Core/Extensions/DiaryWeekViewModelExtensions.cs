using Diary.Core.Dtos;
using Diary.Core.ViewModels.Views;

namespace Diary.Core.Extensions
{
    public static class DiaryWeekViewModelExtensions
    {
        public static DiaryWeekDto ToDto(this DiaryWeekViewModel viewModel)
        {
            return new DiaryWeekDto() { WeekStart = viewModel.WeekStart, Days = viewModel.ChildViewModels.Select(x => (x as DiaryDayViewModel).ToDto()).ToList() };
        }
    }
}
