using System.Text.Json.Serialization;

namespace BaseProject.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SORTBY { ASC, DESC }
}
