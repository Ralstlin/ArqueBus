using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ArqueBus.Abstractions;

namespace ArqueBus
{
    public class EventBus<T, TModel> : IEventBus<T, TModel> where TModel : class
    {
        protected ConcurrentDictionary<T, ConcurrentDictionary<SubscriptionIdentity<T>, ISubscription<T, TModel>>> Subscriptions;

        public EventBus()
        {
            Subscriptions = new ConcurrentDictionary<T, ConcurrentDictionary<SubscriptionIdentity<T>, ISubscription<T, TModel>>>();
        }

        public virtual SubscriptionIdentity<T> Subscribe(T target, Action<TModel> action) 
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!Subscriptions.ContainsKey(target))
            {
                Subscriptions.TryAdd(target, new ConcurrentDictionary<SubscriptionIdentity<T>, ISubscription<T, TModel>>());
            }

            var identity = new SubscriptionIdentity<T>(target);
            Subscriptions[target].TryAdd(identity, new Subscription<T, TModel>(action, identity));
            return identity;
        }

        public virtual SubscriptionIdentity<T> SubscribeOnce(T target, Action<TModel> action)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!Subscriptions.ContainsKey(target))
            {
                Subscriptions.TryAdd(target, new ConcurrentDictionary<SubscriptionIdentity<T>, ISubscription <T, TModel>>());
            }

            var identity = new SubscriptionIdentity<T>(target);

            Subscriptions[target].TryAdd(identity, new Subscription<T, TModel>((data) =>
                {
                    Unsubscribe(identity);
                    action(data);
                }, identity));
            return identity;          
        }

        public virtual void Unsubscribe(SubscriptionIdentity<T> identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (Subscriptions.ContainsKey(identity.Target))
            {
                var allSubscriptions = Subscriptions[identity.Target];
                allSubscriptions.TryRemove(identity, out _);
            }
        }

        public virtual void Unsubscribe(T target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (Subscriptions.ContainsKey(target))
            {
                Subscriptions.TryRemove(target, out _);
            }
        }

        public virtual void Publish(T target, TModel data)
        {
            Task.Run(() => PublishAsync(target, data)).Wait();
        }

        public virtual void Publish<TData>(T target, TData data)
        {
            Task.Run(() => PublishAsync(target, data)).Wait(); 
        }

        public virtual async Task PublishAsync<TData>(T target, TData data)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var converted = data as TModel;

            if (data != null && converted == null)
            {
                throw new InvalidCastException($"Unable to convert type {typeof(TData).Name} to {typeof(TModel).Name}");
            }

            if (Subscriptions.ContainsKey(target))
            {
                foreach (var subs in Subscriptions[target])
                {
                    await subs.Value.Run(target, data as TModel);
                }
            }
        }

        public virtual async Task PublishAsync(T target, TModel data)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!Subscriptions.ContainsKey(target))
            {
                Subscriptions.TryAdd(target, new ConcurrentDictionary<SubscriptionIdentity<T>, ISubscription<T, TModel>>());
            }

            if (Subscriptions.ContainsKey(target))
            {
                foreach (var subs in Subscriptions[target])
                {
                    await subs.Value.Run(target, data);
                }
            }
        }

        /// <summary>
        /// Create a listener to recieve publications. Use using to unsubscribe automatically.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual ISubscriptionListener<T, TModel> CreateListener(T target, DataflowBlockOptions options = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!Subscriptions.ContainsKey(target))
            {
                Subscriptions.TryAdd(target, new ConcurrentDictionary<SubscriptionIdentity<T>, ISubscription<T, TModel>>());
            }
   
            var listener = new SubscriptionListener<T, TModel>(target, this, options);
            Subscriptions[target].TryAdd(listener.SubscriptionIdentity, listener);

            return listener;
        }

        public virtual async Task<List<TOutput>> ListenAsync<TOutput>(T target, DataflowBlockOptions options)
            where TOutput : class, TModel
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (options?.CancellationToken == null)
            {
                throw new ArgumentNullException(nameof(options), "ListenAsync DataflowBlockOptions requires a CancellationToken.");
            }

            var list = new List<TOutput>();

            using (var listener = CreateListener(target, options))
            {
                while (!options.CancellationToken.IsCancellationRequested)
                {
                    var result = await listener.ListenAsync<TOutput>(options.CancellationToken);

                    if (result != null)
                    {
                        list.Add(result);
                    }
                }
            }

            return list;
        }

        public virtual async Task<List<TOutput>> ListenFixedTimesAsync<TOutput>(T target, int timesToListen, DataflowBlockOptions options)
            where TOutput : class, TModel
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if(timesToListen <= 0)
            {
                throw new ArgumentException($"The times to listen for publications must be greater than 0");
            }

            if (options?.CancellationToken == null)
            {
                throw new ArgumentNullException(nameof(options), "ListenAsync DataflowBlockOptions requires a CancellationToken.");
            }

            var list = new List<TOutput>();

            using (var listener = CreateListener(target, options))
            {
                for (int i = 0; i < timesToListen && !options.CancellationToken.IsCancellationRequested; i++)
                {
                    var result = await listener.ListenAsync<TOutput>(options.CancellationToken);

                    if (result != null)
                    {
                        list.Add(result);
                    }
                }
            }

            return list;
        }

        public bool IsSubscribed(SubscriptionIdentity<T> identity)
        {
            return Subscriptions.Any(kvp => kvp.Value.Any(t => t.Key.Identity == identity.Identity));
        }

        public bool HasSubscriptions(T target)
        {
            return Subscriptions.ContainsKey(target) && Subscriptions[target].Count > 0;
        }
    }
}
