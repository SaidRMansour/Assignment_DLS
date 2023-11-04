using EasyNetQ;
using System.Diagnostics;
using System.Collections.Generic;
using Monitoring;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

namespace Helpers
{
    public class RabbitMqService
    {
        private readonly IBus bus;

        public RabbitMqService()
        {
            bus = ConnectionHelper.GetRMQConnection();
        }

        public void SendTracedMessage<T>(string topic, T messagePayload)
        {
            var tracedMessage = new TracedMessage<T>
            {
                Payload = messagePayload,
            };

            var propagator = new TraceContextPropagator();
            var activityContext = Activity.Current?.Context ?? default;
            var propagationContext = new PropagationContext(activityContext, Baggage.Current);

            propagator.Inject(propagationContext, tracedMessage.TracingHeaders, (dict, key, value) => dict[key] = value);

            bus.PubSub.Publish(tracedMessage, topic);
        }

        public void SubscribeToTracedMessage<T>(string subscriptionId, Action<T> handleMessage)
        {
            bus.PubSub.Subscribe<TracedMessage<T>>(subscriptionId, tracedMessage =>
            {
                var propagator = new TraceContextPropagator();

                var parentContext = propagator.Extract(default, tracedMessage.TracingHeaders, (dict, key) =>
                {
                    dict.TryGetValue(key, out var value);
                    return new List<string> { value };
                });

                Baggage.Current = parentContext.Baggage;
                using (var activity = MonitorService.ActivitySource.StartActivity("Handle message", ActivityKind.Consumer, parentContext.ActivityContext))
                {
                    handleMessage(tracedMessage.Payload);
                }
            });
        }
    }
}
