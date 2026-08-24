using System.Collections.Concurrent;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Plugin.Widgets.TripleWhale.Domain;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Consumes nopCommerce events and enqueues Triple Whale sync items
/// </summary>
public class TripleWhaleEventConsumer :
    IConsumer<OrderPlacedEvent>,
    IConsumer<OrderPaidEvent>,
    IConsumer<OrderRefundedEvent>,
    IConsumer<OrderVoidedEvent>,
    IConsumer<OrderStatusChangedEvent>,
    IConsumer<CustomerRegisteredEvent>,
    IConsumer<EntityUpdatedEvent<Customer>>,
    IConsumer<EntityInsertedEvent<Product>>,
    IConsumer<EntityUpdatedEvent<Product>>,
    IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
    IConsumer<EmailSubscribedEvent>,
    IConsumer<ProductSearchEvent>,
    IConsumer<PageRenderingEvent>,
    IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
    IConsumer<ModelPreparedEvent<BaseNopModel>>
{
    private readonly TripleWhaleSettings _settings;
    private readonly TripleWhaleEventBuilder _eventBuilder;
    private readonly TripleWhaleSyncService _syncService;
    private readonly TripleWhalePixelService _pixelService;
    private readonly ICustomerService _customerService;

    private static readonly ConcurrentDictionary<int, DateTime> _lastCustomerSync = new();

    public TripleWhaleEventConsumer(
        TripleWhaleSettings settings,
        TripleWhaleEventBuilder eventBuilder,
        TripleWhaleSyncService syncService,
        TripleWhalePixelService pixelService,
        ICustomerService customerService)
    {
        _settings = settings;
        _eventBuilder = eventBuilder;
        _syncService = syncService;
        _pixelService = pixelService;
        _customerService = customerService;
    }

    /// <summary>
    /// Handle the order placed event
    /// </summary>
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        if (eventMessage?.Order is not { } order)
            return;

        await SyncOrderAsync(order, "order.placed");

        var eventId = await _pixelService.GetCheckoutEventIdAsync();
        _pixelService.ClearCheckoutEventId();

        await SendOrderEventAsync(order, "Purchase", eventId);
        await SendPurchasePixelEventAsync(order, eventId);
    }

    /// <summary>
    /// Handle the order paid event
    /// </summary>
    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        if (eventMessage?.Order is not { } order)
            return;

        await SyncOrderAsync(order, "order.paid");
    }

    /// <summary>
    /// Handle the order refunded event
    /// </summary>
    public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
    {
        if (eventMessage?.Order is not { } order)
            return;

        await SyncOrderAsync(order, "order.refunded");
        await SendOrderEventAsync(order, "Refund");
    }

    /// <summary>
    /// Handle the order voided event
    /// </summary>
    public async Task HandleEventAsync(OrderVoidedEvent eventMessage)
    {
        if (eventMessage?.Order is not { } order)
            return;

        await SyncOrderAsync(order, "order.voided");
    }

    /// <summary>
    /// Handle the order status changed event
    /// </summary>
    public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
    {
        if (eventMessage?.Order is not { } order)
            return;

        await SyncOrderAsync(order, "order.status_changed");
    }

    /// <summary>
    /// Handle the customer registered event
    /// </summary>
    public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
    {
        if (eventMessage?.Customer is not { } customer)
            return;

        if (_settings.EnablePixel)
        {
            await _pixelService.SendContactEventAsync(customer.Email);
            await _pixelService.SendSignUpEventAsync(customer.Email);
        }

        if (!_settings.EnableCustomerSync)
            return;

        var dto = await _eventBuilder.BuildCustomerAsync(customer);
        if (dto is null)
            return;

        await _syncService.EnqueueAsync(TripleWhaleEntityType.Customer, customer.Id.ToString(), "customer.registered", dto);
    }

    /// <summary>
    /// Handle the customer updated event
    /// </summary>
    public async Task HandleEventAsync(EntityUpdatedEvent<Customer> eventMessage)
    {
        if (eventMessage?.Entity is not { } customer)
            return;

        if (!_settings.EnableCustomerSync)
            return;

        // throttle duplicate updates for the same customer within a short window
        var now = DateTime.UtcNow;
        if (_lastCustomerSync.TryGetValue(customer.Id, out var lastSync) && (now - lastSync).TotalSeconds < 60)
            return;

        _lastCustomerSync[customer.Id] = now;

        var dto = await _eventBuilder.BuildCustomerAsync(customer);
        if (dto is null)
            return;

        await _syncService.EnqueueAsync(TripleWhaleEntityType.Customer, customer.Id.ToString(), "customer.updated", dto);
    }

    /// <summary>
    /// Handle the shopping cart item inserted event (add to cart)
    /// </summary>
    public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
    {
        if (eventMessage?.Entity is not { } item)
            return;

        if (!_settings.EnablePixel)
            return;

        if (item.ShoppingCartType == ShoppingCartType.Wishlist)
            await _pixelService.SendAddToWishlistEventAsync(item);
        else
            await _pixelService.SendAddToCartEventAsync(item);
    }

    /// <summary>
    /// Handle the shopping cart item deleted event (remove from cart)
    /// </summary>
    public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
    {
        if (eventMessage?.Entity is not { } item)
            return;

        if (!_settings.EnablePixel)
            return;

        if (item.ShoppingCartType == ShoppingCartType.ShoppingCart)
            await _pixelService.SendRemoveFromCartEventAsync(item);
    }

    /// <summary>
    /// Handle the model prepared event (fires ViewContent on product details pages)
    /// </summary>
    public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
    {
        try
        {
            if (eventMessage?.Model is not ProductDetailsModel productDetailsModel)
                return;

            if (!_settings.EnablePixel)
                return;

            await _pixelService.SendViewContentEventAsync(productDetailsModel.Id, productDetailsModel.Name);
        }
        catch
        {
            // never break the page render
        }
    }

    /// <summary>
    /// Handle the email subscribed event (newsletter)
    /// </summary>
    public async Task HandleEventAsync(EmailSubscribedEvent eventMessage)
    {
        if (eventMessage?.Subscription is not { } subscription)
            return;

        if (!_settings.EnablePixel)
            return;

        await _pixelService.SendContactEventAsync(subscription.Email);
        await _pixelService.SendLeadEventAsync(subscription.Email);
    }

    /// <summary>
    /// Handle the product search event
    /// </summary>
    public async Task HandleEventAsync(ProductSearchEvent eventMessage)
    {
        if (string.IsNullOrWhiteSpace(eventMessage?.SearchTerm))
            return;

        if (!_settings.EnablePixel)
            return;

        await _pixelService.SendSearchEventAsync(eventMessage.SearchTerm);
    }

    /// <summary>
    /// Handle the page rendering event (fires InitiateCheckout on the checkout page)
    /// </summary>
    public async Task HandleEventAsync(PageRenderingEvent eventMessage)
    {
        try
        {
            var routeName = eventMessage?.GetRouteName() ?? string.Empty;

            if (routeName != NopRouteNames.Standard.CHECKOUT && routeName != NopRouteNames.Standard.CHECKOUT_ONE_PAGE)
                return;

            if (!_settings.EnablePixel && !_settings.EnableServerSideEvents)
                return;

            await _pixelService.EnsureCheckoutEventIdAsync();

            if (_settings.EnablePixel)
                await _pixelService.SendInitiateCheckoutEventAsync();
        }
        catch
        {
            // never break the page render
        }
    }

    /// <summary>
    /// Sends a client-side Purchase pixel event with the order summary
    /// </summary>
    private async Task SendPurchasePixelEventAsync(Order order, string eventId)
    {
        if (!_settings.EnablePixel)
            return;

        var purchaseData = await _eventBuilder.BuildPurchasePixelDataAsync(order, eventId);
        await _pixelService.SendPurchaseEventAsync(purchaseData);
    }

    /// <summary>
    /// Enqueues an order sync item when order sync is enabled
    /// </summary>
    private async Task SyncOrderAsync(Order order, string eventType)
    {
        if (!_settings.EnableOrderSync)
            return;

        var dto = await _eventBuilder.BuildOrderAsync(order);
        await _syncService.EnqueueAsync(TripleWhaleEntityType.Order, order.Id.ToString(), eventType, dto);
    }

    /// <summary>
    /// Enqueues a server-side conversion event when server-side events are enabled
    /// </summary>
    private async Task SendOrderEventAsync(Order order, string eventName, string eventId = null)
    {
        if (!_settings.EnableServerSideEvents)
            return;

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var offlineEvent = await _eventBuilder.BuildOfflineEventAsync(
            "custom", eventName, customer?.Email, null, customer?.Id.ToString(), null, null, null, null, eventId);

        await _syncService.EnqueueAsync(TripleWhaleEntityType.Event, order.Id.ToString(), eventName, offlineEvent);
    }

    /// <summary>
    /// Handle the product inserted event
    /// </summary>
    public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
    {
        await SyncProductAsync(eventMessage?.Entity);
    }

    /// <summary>
    /// Handle the product updated event
    /// </summary>
    public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
    {
        await SyncProductAsync(eventMessage?.Entity);
    }

    /// <summary>
    /// Enqueues a product sync item when product sync is enabled
    /// </summary>
    private async Task SyncProductAsync(Product product)
    {
        if (product is null)
            return;

        if (!_settings.EnableProductSync)
            return;

        var dto = await _eventBuilder.BuildProductAsync(product);
        await _syncService.EnqueueAsync(TripleWhaleEntityType.Product, product.Id.ToString(), "product.updated", dto);
    }
}
