using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Diary.Models
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
