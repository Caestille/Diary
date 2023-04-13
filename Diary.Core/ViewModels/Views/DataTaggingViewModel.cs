using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using CoreUtilities.HelperClasses;
using System.Text.Json;
using System.Collections.ObjectModel;
using Diary.Core.ViewModels.Base;
using Diary.Core.Messages;
using Diary.Core.Messages.Base;
using Diary.Core.Models;

namespace Diary.Core.ViewModels.Views
{
    public class DataTaggingViewModel : ViewModelBase
    {
        private string tagsWriteDirectory => Path.Combine(workingDirectory, "CustomTags.json");

        public static List<CustomTag> PublicCustomTags = new();

        public ICommand AddCustomTagCommand => new RelayCommand(() =>
        {
            CustomTags.Add(ProposedTag);
            ProposedTag = new CustomTag();
			OnPropertyChanged(nameof(CustomTags));
			SynchroniseTags();
        });

        public ICommand DeleteCustomTagCommand => new RelayCommand<CustomTag>((tag) =>
        {
            CustomTags.Remove(tag);
			OnPropertyChanged(nameof(CustomTags));
			SynchroniseTags();
        });
        public ICommand CustomTagEditorKeyDownCommand => new RelayCommand<object>((args) => CustomTagEditorKeyDown(args));

        private CustomTag proposedTag;
        public CustomTag ProposedTag
        {
            get => proposedTag;
            set => SetProperty(ref proposedTag, value);
        }

        private string workingDirectory;

        private ObservableCollection<CustomTag> customTags = new();
        public ObservableCollection<CustomTag> CustomTags
        {
            get => customTags;
            set
            {
                SetProperty(ref customTags, value);
                PublicCustomTags = new List<CustomTag>(customTags);
            }
        }

        private bool canAddTag;
        public bool CanAddTag
        {
            get => canAddTag;
            set => SetProperty(ref canAddTag, value);
        }

        public DataTaggingViewModel(string workingDirectory)
            : base("Tagging")
        {
            this.workingDirectory = workingDirectory;
            ProposedTag = new CustomTag();

            if (File.Exists(tagsWriteDirectory))
            {
                CustomTags = new RangeObservableCollection<CustomTag>(
                    JsonSerializer.Deserialize<List<CustomTag>>(File.ReadAllText(tagsWriteDirectory)));
            }
        }

        protected override void BindMessages()
        {
            Messenger.Register<RequestSyncTagsMessage>(this, (sender, message) =>
            {
                SynchroniseTags();
            });
            base.BindMessages();
        }

        protected override void OnShutdownStart(object? sender, EventArgs e)
        {
            File.WriteAllText(tagsWriteDirectory, JsonSerializer.Serialize(CustomTags));
            base.OnShutdownStart(sender, e);
        }

        private void CustomTagEditorKeyDown(object args)
        {
            CanAddTag = !CustomTags.Select(x => x.Tag).Contains(proposedTag.Tag)
                && !string.IsNullOrWhiteSpace(proposedTag.Tag)
                && !string.IsNullOrEmpty(proposedTag.Tag);

            if (CanAddTag && args is KeyEventArgs e && (e.Key == Key.Enter || e.Key == Key.Escape) && e.Key == Key.Enter)
            {
                AddCustomTagCommand.Execute(null);
            }
        }

        private void SynchroniseTags()
        {
            PublicCustomTags = new List<CustomTag>(customTags);
            Messenger.Send(new SyncTagsMessage(CustomTags));
        }
    }
}