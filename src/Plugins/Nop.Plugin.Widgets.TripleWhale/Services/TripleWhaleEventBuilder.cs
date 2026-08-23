using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.TripleWhale.Models.Dto;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Builds Triple Whale payloads from nopCommerce entities
/// </summary>
public class TripleWhaleEventBuilder
{
    private readonly TripleWhaleSettings _settings;
    private readonly IStoreContext _storeContext;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;

    public TripleWhaleEventBuilder(TripleWhaleSettings settings,
        IStoreContext storeContext,
        IOrderService orderService,
        IProductService productService,
        ICustomerService customerService,
        IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService)
    {
        _settings = settings;
        _storeContext = storeContext;
        _orderService = orderService;
        _productService = productService;
        _customerService = customerService;
        _addressService = addressService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
    }

    /// <summary>
    /// Builds an order record DTO from a nopCommerce order
    /// </summary>
    public async Task<OrderRecordDto> BuildOrderAsync(Order order)
    {
        var shop = await GetShopAsync();
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

        var lineItems = new List<OrderLineItemDto>();
        foreach (var item in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            lineItems.Add(new OrderLineItemDto
            {
                Id = item.Id.ToString(),
                Name = product?.Name,
                Price = item.UnitPriceInclTax,
                Quantity = item.Quantity,
                ProductId = item.ProductId.ToString(),
                ProductName = product?.Name,
                VariantId = item.ProductId.ToString(),
                Sku = product?.Sku
            });
        }

        var address = order.ShippingAddressId.HasValue
            ? await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value)
            : null;

        var billingAddress = customer is not null && customer.BillingAddressId.HasValue
            ? await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value)
            : null;

        return new OrderRecordDto
        {
            Shop = shop,
            OrderId = order.Id.ToString(),
            OrderRevenue = order.OrderTotal,
            Platform = "nopcommerce",
            PlatformAccountId = shop,
            CreatedAt = FormatDate(order.CreatedOnUtc),
            Currency = order.CustomerCurrencyCode,
            Customer = customer is null ? null : new OrderCustomerDto
            {
                Id = customer.Id.ToString(),
                Email = customer.Email,
                FirstName = !string.IsNullOrWhiteSpace(billingAddress?.FirstName) ? billingAddress.FirstName : customer.FirstName,
                LastName = !string.IsNullOrWhiteSpace(billingAddress?.LastName) ? billingAddress.LastName : customer.LastName
            },
            Name = $"Order #{order.CustomOrderNumber}",
            LineItems = lineItems,
            ShippingAddress = await BuildAddressAsync(address),
            ShippingPrice = order.OrderShippingInclTax,
            TotalDiscounts = order.OrderDiscount,
            Taxes = order.OrderTax,
            PaymentGatewayNames = string.IsNullOrEmpty(order.PaymentMethodSystemName)
                ? null
                : new List<string> { order.PaymentMethodSystemName },
            Status = order.OrderStatus.ToString(),
            Void = order.OrderStatus == OrderStatus.Cancelled,
            Refunds = order.RefundedAmount > 0
                ? new List<OrderRefundDto>
                {
                    new OrderRefundDto
                    {
                        RefundId = order.Id + "-refund",
                        RefundedAt = FormatDate(DateTime.UtcNow),
                        TotalRefund = order.RefundedAmount,
                        LineItems = BuildRefundLineItems(lineItems, order.RefundedAmount)
                    }
                }
                : null,
            TaxesIncluded = true
        };
    }

    /// <summary>
    /// Builds a customer record DTO from a nopCommerce customer
    /// </summary>
    public async Task<CustomerRecordDto> BuildCustomerAsync(Customer customer)
    {
        // skip nopCommerce built-in bot customers (search engine / background task placeholders)
        if (customer.Email is not null && customer.Email.StartsWith("builtin@", StringComparison.OrdinalIgnoreCase))
            return null;

        var shop = await GetShopAsync();
        var address = customer.BillingAddressId.HasValue
            ? await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value)
            : null;

        var firstName = !string.IsNullOrWhiteSpace(address?.FirstName) ? address.FirstName : customer.FirstName;
        var lastName = !string.IsNullOrWhiteSpace(address?.LastName) ? address.LastName : customer.LastName;
        var displayName = (firstName + " " + lastName).Trim();

        return new CustomerRecordDto
        {
            Shop = shop,
            CustomerId = customer.Id.ToString(),
            Platform = "nopcommerce",
            PlatformAccountId = shop,
            EmailAddress = customer.Email,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = displayName,
            Address1 = address?.Address1,
            Address2 = address?.Address2,
            City = address?.City,
            Zip = address?.ZipPostalCode,
            CreatedAt = FormatDate(customer.CreatedOnUtc),
            TaxExempt = customer.IsTaxExempt
        };
    }

    /// <summary>
    /// Builds a product record DTO from a nopCommerce product
    /// </summary>
    public async Task<ProductRecordDto> BuildProductAsync(Product product)
    {
        var shop = await GetShopAsync();

        return new ProductRecordDto
        {
            Shop = shop,
            ProductId = product.Id.ToString(),
            ProductTitle = product.Name,
            Platform = "nopcommerce",
            PlatformAccountId = shop,
            Variants = new List<ProductVariantDto>
            {
                new ProductVariantDto
                {
                    Id = product.Id.ToString(),
                    VariantId = product.Id.ToString(),
                    VariantName = product.Name,
                    VariantTitle = product.Name,
                    Name = product.Name,
                    Sku = product.Sku,
                    VariantPrice = product.Price
                }
            },
            Tags = new List<string>()
        };
    }

    /// <summary>
    /// Builds an offline / server-side event DTO
    /// </summary>
    public async Task<OfflineEventDto> BuildOfflineEventAsync(string type, string eventName, string email, string phone,
        string externalId, string source, string campaign, string ad, string adset, string eventId = null)
    {
        var shop = await GetShopAsync();

        return new OfflineEventDto
        {
            Shop = shop,
            Type = type,
            EventName = eventName,
            ProfileIdentifiers = new ProfileIdentifiersDto
            {
                Email = email,
                Phone = phone,
                ExternalId = externalId
            },
            EventId = string.IsNullOrWhiteSpace(eventId) ? Guid.NewGuid().ToString("N") : eventId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Source = source,
            Campaign = campaign,
            Ad = ad,
            Adset = adset
        };
    }

    /// <summary>
    /// Builds the client-side Purchase pixel event data from a nopCommerce order
    /// </summary>
    public async Task<Dictionary<string, object>> BuildPurchasePixelDataAsync(Order order, string eventId = null)
    {
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

        var products = new List<object>();
        foreach (var item in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            products.Add(new Dictionary<string, object>
            {
                ["id"] = item.ProductId.ToString(),
                ["name"] = product?.Name,
                ["sku"] = product?.Sku,
                ["price"] = item.UnitPriceInclTax,
                ["quantity"] = item.Quantity
            });
        }

        return new Dictionary<string, object>
        {
            ["event_id"] = eventId,
            ["orderId"] = order.Id.ToString(),
            ["revenue"] = order.OrderTotal,
            ["currency"] = order.CustomerCurrencyCode,
            ["products"] = products,
            ["taxes"] = order.OrderTax,
            ["shipping"] = order.OrderShippingInclTax
        };
    }

    /// <summary>
    /// Builds a shipping/billing address DTO
    /// </summary>
    private async Task<OrderAddressDto> BuildAddressAsync(Address address)
    {
        if (address is null)
            return null;

        var country = address.CountryId.HasValue
            ? await _countryService.GetCountryByIdAsync(address.CountryId.Value)
            : null;
        var stateProvince = address.StateProvinceId.HasValue
            ? await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value)
            : null;

        return new OrderAddressDto
        {
            Address1 = address.Address1,
            Address2 = address.Address2,
            Zip = address.ZipPostalCode,
            City = address.City,
            CountryCode = country?.TwoLetterIsoCode,
            ProvinceCode = stateProvince?.Abbreviation
        };
    }

    /// <summary>
    /// Builds refund line items by distributing the refunded amount proportionally across the order items
    /// </summary>
    private static List<OrderLineItemDto> BuildRefundLineItems(List<OrderLineItemDto> lineItems, decimal refundedAmount)
    {
        if (lineItems is null || !lineItems.Any())
            return lineItems;

        var totalOrderAmount = lineItems.Sum(item => item.Price * item.Quantity);
        if (totalOrderAmount <= 0)
            return lineItems;

        var ratio = refundedAmount / totalOrderAmount;

        return lineItems.Select(item => new OrderLineItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Price = Math.Round(item.Price * ratio, 4),
            Quantity = item.Quantity,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            VariantId = item.VariantId,
            Sku = item.Sku
        }).ToList();
    }

    /// <summary>
    /// Gets the Triple Whale shop domain: the configured ShopUrl or the current store host
    /// </summary>
    private async Task<string> GetShopAsync()
    {
        if (!string.IsNullOrWhiteSpace(_settings.ShopUrl))
            return TripleWhaleStoreHelper.NormalizeShop(_settings.ShopUrl);

        var store = await _storeContext.GetCurrentStoreAsync();
        return TripleWhaleStoreHelper.NormalizeShop(TripleWhaleStoreHelper.GetHost(store?.Url));
    }

    /// <summary>
    /// Formats a date as ISO 8601 UTC
    /// </summary>
    private static string FormatDate(DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
    }
}
