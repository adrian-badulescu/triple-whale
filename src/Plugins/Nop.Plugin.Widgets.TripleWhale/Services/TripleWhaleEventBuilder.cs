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
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;

    public TripleWhaleEventBuilder(TripleWhaleSettings settings,
        IOrderService orderService,
        IProductService productService,
        ICustomerService customerService,
        IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService)
    {
        _settings = settings;
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
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

        var lineItems = new List<OrderLineItemDto>();
        foreach (var item in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            lineItems.Add(new OrderLineItemDto
            {
                Id = item.Id.ToString(),
                Price = item.UnitPriceInclTax,
                Quantity = item.Quantity,
                ProductId = item.ProductId.ToString(),
                ProductName = product?.Name,
                Sku = product?.Sku
            });
        }

        var address = order.ShippingAddressId.HasValue
            ? await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value)
            : null;

        return new OrderRecordDto
        {
            Shop = _settings.ShopUrl,
            OrderId = order.Id.ToString(),
            OrderRevenue = order.OrderTotal,
            Platform = "nopcommerce",
            PlatformAccountId = _settings.ShopUrl,
            CreatedAt = FormatDate(order.CreatedOnUtc),
            Currency = order.CustomerCurrencyCode,
            Customer = customer is null ? null : new OrderCustomerDto
            {
                Id = customer.Id.ToString(),
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName
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
            TaxesIncluded = true
        };
    }

    /// <summary>
    /// Builds a customer record DTO from a nopCommerce customer
    /// </summary>
    public async Task<CustomerRecordDto> BuildCustomerAsync(Customer customer)
    {
        var address = customer.BillingAddressId.HasValue
            ? await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value)
            : null;

        return new CustomerRecordDto
        {
            Shop = _settings.ShopUrl,
            CustomerId = customer.Id.ToString(),
            Platform = "nopcommerce",
            PlatformAccountId = _settings.ShopUrl,
            EmailAddress = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            DisplayName = customer.FirstName + " " + customer.LastName,
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
        return new ProductRecordDto
        {
            Shop = _settings.ShopUrl,
            ProductId = product.Id.ToString(),
            ProductTitle = product.Name,
            Platform = "nopcommerce",
            PlatformAccountId = _settings.ShopUrl,
            Variants = new List<ProductVariantDto>
            {
                new ProductVariantDto
                {
                    Id = product.Id.ToString(),
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
    public OfflineEventDto BuildOfflineEvent(string type, string eventName, string email, string phone,
        string pixelClientId, string source, string campaign, string ad, string adset)
    {
        return new OfflineEventDto
        {
            Shop = _settings.ShopUrl,
            Type = type,
            EventName = eventName,
            ProfileIdentifiers = new ProfileIdentifiersDto
            {
                Email = email,
                Phone = phone,
                PixelClientId = pixelClientId
            },
            Source = source,
            Campaign = campaign,
            Ad = ad,
            Adset = adset
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
    /// Formats a date as ISO 8601 UTC
    /// </summary>
    private static string FormatDate(DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
    }
}
