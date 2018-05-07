using System;

namespace ArqueBus
{
    public class SubscriptionIdentity<T>
    {
        public SubscriptionIdentity(T target)
        {
            Identity = Guid.NewGuid();
            Target = target;
        }
        
        public Guid Identity { get; }
        public T Target { get; }
    }
}
