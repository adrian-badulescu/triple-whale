using System.Text.Json.Serialization;

namespace Nop.Plugin.Widgets.TripleWhale.Models.Dto;

/// <summary>
/// Product record payload for POST /data-in/products
/// </summary>
public class ProductRecordDto
{
    [JsonPropertyName("shop")] public string Shop { get; set; }
    [JsonPropertyName("product_id")] public string ProductId { get; set; }
    [JsonPropertyName("product_title")] public string ProductTitle { get; set; }
    [JsonPropertyName("platform")] public string Platform { get; set; }
    [JsonPropertyName("platform_account_id")] public string PlatformAccountId { get; set; }
    [JsonPropertyName("variants")] public List<ProductVariantDto> Variants { get; set; }
    [JsonPropertyName("collections")] public List<string> Collections { get; set; }
    [JsonPropertyName("product_tags")] public List<string> Tags { get; set; }
}

public class ProductVariantDto
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("sku")] public string Sku { get; set; }
    [JsonPropertyName("variant_price")] public decimal VariantPrice { get; set; }
}
