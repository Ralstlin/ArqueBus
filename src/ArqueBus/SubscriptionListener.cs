using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ArqueBus.Abstractions;

namespace ArqueBus
{
    public sealed class SubscriptionListener<T, TModel> : ISubscriptionListener<T, TModel>
        where TModel : class
    {
        private readonly BufferBlock<TModel> buffer;
        private readonly IEventBus<T, TModel> bus;

        public SubscriptionIdentity<T> SubscriptionIdentity { get; private set; }


        public SubscriptionListener(T target, IEventBus<T, TModel> bus, DataflowBlockOptions options = null)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));

            SubscriptionIdentity = new SubscriptionIdentity<T>(target);
            buffer = options == null ? new BufferBlock<TModel>() : new BufferBlock<TModel>(options);
        }

        public async Task Run(T target, TModel data)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            await buffer.SendAsync(data);
        }

        public async Task<TModel> ListenAsync()
        {           
            return await buffer.ReceiveAsync();
        }

        public async Task<TModel> ListenAsync(CancellationToken token)
        {
            return await buffer.ReceiveAsync(token);
        }

        public async Task<TData> ListenAsync<TData>() where TData : class
        {
            var model = await ListenAsync();
            var converted = model as TData;
            if (model != null && converted == null)
            {
                throw new InvalidCastException($"Cannot convert type of {typeof(TModel)} to {typeof(TData)}");
            }

            return converted;
        }

        public async Task<TData> ListenAsync<TData>(CancellationToken token) where TData : class
        {
            var model = await ListenAsync(token);
            var converted = model as TData;
            if (model != null && converted == null)
            {
                throw new InvalidCastException($"Cannot convert type of {typeof(TModel)} to {typeof(TData)}");
            }

            return converted;
        }

        public void Dispose()
        {
            bus.Unsubscribe(SubscriptionIdentity);
        }
    }
}
