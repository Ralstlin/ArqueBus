using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ArqueBus.Abstractions
{
    public interface IEventBus<T, TModel> where TModel : class
    {
        SubscriptionIdentity<T> Subscribe(T target, Action<TModel> action);
        SubscriptionIdentity<T> SubscribeOnce(T target, Action<TModel> action);
        void Publish(T target, TModel data);
        Task PublishAsync(T target, TModel data);

        void Unsubscribe(SubscriptionIdentity<T> identity);
        void Unsubscribe(T target);

        bool IsSubscribed(SubscriptionIdentity<T> identity);
        bool HasSubscriptions(T target);

        ISubscriptionListener<T, TModel> CreateListener(T target, DataflowBlockOptions options = null);
        Task<List<TOutput>> ListenAsync<TOutput>(T target, DataflowBlockOptions options) where TOutput : class, TModel;

        Task<List<TOutput>> ListenFixedTimesAsync<TOutput>(T target, int timesToListen, DataflowBlockOptions options) where TOutput : class, TModel;
    }
}
