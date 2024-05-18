using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace SuperAdmin.Service.Models.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum Applications
{
    Job = 1,
    Pays,
}
