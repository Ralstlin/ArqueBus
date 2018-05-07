using System.Threading.Tasks;

namespace ArqueBus.Abstractions
{
    public interface ISubscription<T, TModel> where TModel : class
    {
        SubscriptionIdentity<T> SubscriptionIdentity { get; }
        Task Run(T target, TModel data);
    }
}
