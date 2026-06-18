using Microsoft.Extensions.DependencyInjection;

namespace CTI.AIDev.Common
{ 
    public interface IServiceConfigurator
    {
        public ServiceLifetime Transient { get; }
        public ServiceLifetime Scoped { get; }
        public ServiceLifetime Singleton { get; }

        public bool HasService<TService>()
            where TService : class;

        public bool ConfigureService<TService, TImplementation>(ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService;
    }
}