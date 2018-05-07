using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArqueBus.Abstractions
{
    public interface ISubscriptionListener<T, TModel> : ISubscription<T, TModel>, IDisposable where TModel : class
    {
        Task<TModel> ListenAsync();
        Task<TModel> ListenAsync(CancellationToken token);
        Task<TData> ListenAsync<TData>() where TData : class;
        Task<TData> ListenAsync<TData>(CancellationToken token) where TData : class;
    }
}