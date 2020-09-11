using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Orders;

namespace SmartStore.Verizon
{
    public class OrderPlacedEventConsumer : IConsumer
    {
        private readonly VerizonSettings _verizonSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IOrderService _orderService;
        private readonly ICommonServices _services;

        public OrderPlacedEventConsumer(VerizonSettings verizonSettings,
            IPluginFinder pluginFinder,
            IOrderService orderService,
            ICommonServices services)
        {
            _verizonSettings = verizonSettings;
            _pluginFinder = pluginFinder;
            _orderService = orderService;
            _services = services;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            //is enabled?
            if (!_verizonSettings.Enabled)
                return;

            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.Verizon");
            if (pluginDescriptor == null)
                return;

            var store = _services.StoreContext.CurrentStore;

            if (!(store.Id == 0 || _services.Settings.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(store.Id, true)))
                return;

            if (!(pluginDescriptor.Instance() is VerizonSmsProvider plugin))
                return;

            var order = eventMessage.Order;

            //send SMS
            var message = _services.Localization.GetResource("Plugins.Sms.Verizon.OrderPlacedMessage").FormatInvariant(order.GetOrderNumber());
            if (plugin.SendSms(message))
            {
                _orderService.AddOrderNote(order, _services.Localization.GetResource("Plugins.Sms.Verizon.OrderPlacedMessageSend"));
            }
        }
    }
}