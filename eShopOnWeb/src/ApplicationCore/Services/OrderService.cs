using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IAsyncRepository<Order> _orderRepository;
        private readonly IUriComposer _uriComposer;
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly IAsyncRepository<CatalogItem> _itemRepository;

        public OrderService(IAsyncRepository<Basket> basketRepository,
            IAsyncRepository<CatalogItem> itemRepository,
            IAsyncRepository<Order> orderRepository,
            IUriComposer uriComposer)
        {
            _orderRepository = orderRepository;
            _uriComposer = uriComposer;
            _basketRepository = basketRepository;
            _itemRepository = itemRepository;
        }

        public async Task CreateOrderAsync(int basketId, Address shippingAddress)
        {
            var basketSpec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

            Guard.Against.NullBasket(basketId, basket);
            Guard.Against.EmptyBasketOnCheckout(basket.Items);

            var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
            var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

            var items = basket.Items.Select(basketItem =>
            {
                var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
                var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
                var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
                return orderItem;
            }).ToList();

            var order = new Order(basket.BuyerId, shippingAddress, items);

            await _orderRepository.AddAsync(order);

            using var httpclient = new HttpClient();

            var sb = new StringBuilder();
            foreach (var item in order.OrderItems)
            {
                sb
                    .Append(item.ItemOrdered.ProductName)
                    .Append(", ");
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("BuyerId", order.BuyerId.ToString()),
                new KeyValuePair<string, string>("OrderDate", order.OrderDate.ToString()),
                new KeyValuePair<string, string>("ShipToAddress", $"{order.ShipToAddress.Country}, {order.ShipToAddress.City}, {order.ShipToAddress.State}, {order.ShipToAddress.Street}, {order.ShipToAddress.ZipCode}"),
                new KeyValuePair<string, string>("OrderItems", sb.ToString())
            });

            await httpclient.PostAsync(@"https://delveryorderprocessor.azurewebsites.net/api/DelveryOrderProcessor?code=Xy61hT0y7dXRTD7ZTE9AaRSBru2TFXUaTaetGcEMyHraWf4rvsFBoQ==", content);
        }
    }
}
