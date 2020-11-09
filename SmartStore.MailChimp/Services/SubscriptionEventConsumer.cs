using System;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.MailChimp.Data;

namespace SmartStore.MailChimp.Services
{
    public class SubscriptionEventConsumer : IConsumer
    {
        private readonly ISubscriptionEventQueueingService _service;
        private readonly IPluginFinder _pluginFinder;

        public SubscriptionEventConsumer(ISubscriptionEventQueueingService service,
            IPluginFinder pluginFinder)
        {
            this._service = service;
            this._pluginFinder = pluginFinder;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(EmailSubscribedEvent eventMessage)
        {
            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.MailChimp");
            if (pluginDescriptor == null)
                return;

            _service.Insert(new MailChimpEventQueueRecord { Email = eventMessage.Email, IsSubscribe = true, CreatedOnUtc = DateTime.UtcNow });
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(EmailUnsubscribedEvent eventMessage)
        {
            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.MailChimp");
            if (pluginDescriptor == null)
                return;

            _service.Insert(new MailChimpEventQueueRecord { Email = eventMessage.Email, IsSubscribe = false, CreatedOnUtc = DateTime.UtcNow });
        }
    }
}