using System;
using System.Threading.Tasks;
using ArqueBus.Abstractions;

namespace ArqueBus
{
    public class Subscription<T, TModel> : ISubscription<T, TModel> 
        where TModel : class
    {
        private readonly Action<TModel> action;
        public SubscriptionIdentity<T> SubscriptionIdentity { get; }

        public Subscription(Action<TModel> action, SubscriptionIdentity<T> identity)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            SubscriptionIdentity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public async Task Run(T target, TModel data)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (action != null)
            {
                await Task.Run(() => action(data));
            }
        }
    }
}
