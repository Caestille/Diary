﻿using Diary.Dtos;
using Diary.ViewModels.Views;

namespace Diary.Extensions
{
    internal static class DiaryEntryViewModelExtensions
    {
        public static DiaryEntryDto ToDto(this DiaryEntryViewModel viewModel)
        {
            return new DiaryEntryDto()
            {
                Entry = viewModel.EntryText,
                Tag = (viewModel.Tag?.Tag ?? ""),
                Start = viewModel.StartTime,
                End = viewModel.EndTime
            };
        }
    }
}
