using System.Text.Json.Serialization;

namespace Nop.Plugin.Widgets.TripleWhale.Models.Dto;

/// <summary>
/// Customer record payload for POST /data-in/customers
/// </summary>
public class CustomerRecordDto
{
    [JsonPropertyName("shop")] public string Shop { get; set; }
    [JsonPropertyName("customer_id")] public string CustomerId { get; set; }
    [JsonPropertyName("platform")] public string Platform { get; set; }
    [JsonPropertyName("platform_account_id")] public string PlatformAccountId { get; set; }
    [JsonPropertyName("email_address")] public string EmailAddress { get; set; }
    [JsonPropertyName("first_name")] public string FirstName { get; set; }
    [JsonPropertyName("last_name")] public string LastName { get; set; }
    [JsonPropertyName("display_name")] public string DisplayName { get; set; }
    [JsonPropertyName("phone")] public string Phone { get; set; }
    [JsonPropertyName("address1")] public string Address1 { get; set; }
    [JsonPropertyName("address2")] public string Address2 { get; set; }
    [JsonPropertyName("city")] public string City { get; set; }
    [JsonPropertyName("country")] public string Country { get; set; }
    [JsonPropertyName("country_code")] public string CountryCode { get; set; }
    [JsonPropertyName("state")] public string State { get; set; }
    [JsonPropertyName("state_code")] public string StateCode { get; set; }
    [JsonPropertyName("zip")] public string Zip { get; set; }
    [JsonPropertyName("amount_spent")] public decimal AmountSpent { get; set; }
    [JsonPropertyName("orders")] public int Orders { get; set; }
    [JsonPropertyName("customer_tags")] public List<string> CustomerTags { get; set; }
    [JsonPropertyName("created_at")] public string CreatedAt { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; }
    [JsonPropertyName("tax_exempt")] public bool TaxExempt { get; set; }
}
