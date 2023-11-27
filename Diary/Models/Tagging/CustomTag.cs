using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Diary.Models.Tagging
{
	public class CustomTag : ObservableObject
	{
		public CustomTag() { }

		[JsonPropertyName("tag")]
		public string Tag { get; set; }

		[JsonPropertyName("include")]
		public bool IsIncluded { get; set; }
	}
}
