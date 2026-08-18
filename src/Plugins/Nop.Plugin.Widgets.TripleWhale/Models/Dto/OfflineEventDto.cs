using System.Text.Json.Serialization;

namespace Nop.Plugin.Widgets.TripleWhale.Models.Dto;

/// <summary>
/// Offline / server-side event payload for POST /data-in/event
/// </summary>
public class OfflineEventDto
{
    [JsonPropertyName("shop")] public string Shop { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("profileIdentifiers")] public ProfileIdentifiersDto ProfileIdentifiers { get; set; }
    [JsonPropertyName("eventId")] public string EventId { get; set; }
    [JsonPropertyName("timestamp")] public long? Timestamp { get; set; }
    [JsonPropertyName("event_name")] public string EventName { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; }
    [JsonPropertyName("campaign")] public string Campaign { get; set; }
    [JsonPropertyName("ad")] public string Ad { get; set; }
    [JsonPropertyName("adset")] public string Adset { get; set; }
}

public class ProfileIdentifiersDto
{
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("phone")] public string Phone { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; }
}
