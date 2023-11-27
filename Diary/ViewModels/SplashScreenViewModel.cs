using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Diary.ViewModels
{
	public class SplashScreenViewModel : ObservableObject
	{
        private string applicationName;
        public string ApplicationName
        {
            get => applicationName;
            set => SetProperty(ref applicationName, value);
        }

        private Uri imageUri;
		public Uri ImageUri
        {
			get => imageUri;
			set => SetProperty(ref imageUri, value);
        }

        private string copyrightNotice;
        public string CopyrightNotice
        {
            get => copyrightNotice;
            set => SetProperty(ref copyrightNotice, value);
        }

        private string updateStep;
		public string UpdateStep
		{
			get => updateStep;
			set => SetProperty(ref updateStep, value);
        }

        private double progressPercent;
        public double ProgressPercent
        {
            get => progressPercent;
            set => SetProperty(ref progressPercent, value);
        }
    }
}
