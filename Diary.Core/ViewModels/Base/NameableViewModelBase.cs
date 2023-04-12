using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

namespace Diary.Core.ViewModels.Base
{
	public class NameableViewModelBase : ViewModelBase
	{
		private string defaultName;

		public ICommand EditNameCommand => new RelayCommand(EditName);
		public ICommand NameEditorKeyDownCommand => new RelayCommand<object>(NameEditorKeyDown);

		private bool isEditingName;
		public bool IsEditingName
		{
			get => isEditingName;
			set => SetProperty(ref isEditingName, value);
		}

		private string temporaryName;
		public string TemporaryName
		{
			get => temporaryName;
			set => SetProperty(ref temporaryName, value);
		}

		public NameableViewModelBase(
			string name, string defaultName, Func<ViewModelBase> createChild = null)
			: base(name, createChild)
		{
			this.defaultName = defaultName;
		}

		protected virtual void OnCommitNameUpdate()
		{

		}

		private void EditName()
		{
			if (!IsEditingName)
			{
				TemporaryName = Name == defaultName ? string.Empty : Name;
				IsEditingName = true;
			}
			else
			{
				Name = TemporaryName;
				IsEditingName = false;
			}
		}

		private void NameEditorKeyDown(object args)
		{
			if (args is KeyEventArgs e && (e.Key == Key.Enter || e.Key == Key.Escape))
			{
				if (e.Key == Key.Enter)
				{
					Name = TemporaryName;
					OnCommitNameUpdate();
				}

				IsEditingName = false;
			}
		}
	}
}
