using System.Text.Json.Serialization;

namespace Nop.Plugin.Widgets.TripleWhale.Models.Dto;

/// <summary>
/// Order record payload for POST /data-in/orders
/// </summary>
public class OrderRecordDto
{
    [JsonPropertyName("shop")] public string Shop { get; set; }
    [JsonPropertyName("order_id")] public string OrderId { get; set; }
    [JsonPropertyName("order_revenue")] public decimal OrderRevenue { get; set; }
    [JsonPropertyName("platform")] public string Platform { get; set; }
    [JsonPropertyName("platform_account_id")] public string PlatformAccountId { get; set; }
    [JsonPropertyName("created_at")] public string CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public string UpdatedAt { get; set; }
    [JsonPropertyName("currency")] public string Currency { get; set; }
    [JsonPropertyName("customer")] public OrderCustomerDto Customer { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("line_items")] public List<OrderLineItemDto> LineItems { get; set; }
    [JsonPropertyName("refunds")] public List<OrderRefundDto> Refunds { get; set; }
    [JsonPropertyName("shipping_address")] public OrderAddressDto ShippingAddress { get; set; }
    [JsonPropertyName("shipping_price")] public decimal ShippingPrice { get; set; }
    [JsonPropertyName("total_discounts")] public decimal TotalDiscounts { get; set; }
    [JsonPropertyName("taxes")] public decimal Taxes { get; set; }
    [JsonPropertyName("payment_gateway_names")] public List<string> PaymentGatewayNames { get; set; }
    [JsonPropertyName("source_name")] public string SourceName { get; set; }
    [JsonPropertyName("tags")] public List<string> Tags { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; }
    [JsonPropertyName("taxes_included")] public bool TaxesIncluded { get; set; }
    [JsonPropertyName("void")] public bool Void { get; set; }
    [JsonPropertyName("discount_codes")] public List<OrderDiscountCodeDto> DiscountCodes { get; set; }
}

public class OrderCustomerDto
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("phone")] public string Phone { get; set; }
    [JsonPropertyName("first_name")] public string FirstName { get; set; }
    [JsonPropertyName("last_name")] public string LastName { get; set; }
}

public class OrderLineItemDto
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("quantity")] public int Quantity { get; set; }
    [JsonPropertyName("product_id")] public string ProductId { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; }
    [JsonPropertyName("variant_id")] public string VariantId { get; set; }
    [JsonPropertyName("variant_name")] public string VariantName { get; set; }
    [JsonPropertyName("sku")] public string Sku { get; set; }
}

public class OrderRefundDto
{
    [JsonPropertyName("refund_id")] public string RefundId { get; set; }
    [JsonPropertyName("refunded_at")] public string RefundedAt { get; set; }
    [JsonPropertyName("total_refund")] public decimal TotalRefund { get; set; }
    [JsonPropertyName("line_items")] public List<OrderLineItemDto> LineItems { get; set; }
}

public class OrderAddressDto
{
    [JsonPropertyName("address_1")] public string Address1 { get; set; }
    [JsonPropertyName("address_2")] public string Address2 { get; set; }
    [JsonPropertyName("zip")] public string Zip { get; set; }
    [JsonPropertyName("city")] public string City { get; set; }
    [JsonPropertyName("country_code")] public string CountryCode { get; set; }
    [JsonPropertyName("province_code")] public string ProvinceCode { get; set; }
}

public class OrderDiscountCodeDto
{
    [JsonPropertyName("code")] public string Code { get; set; }
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }
}
