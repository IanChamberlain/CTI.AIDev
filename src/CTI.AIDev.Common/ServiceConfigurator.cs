/*
 * Purpose:
 *   Wraps common service-registration operations behind the shared configurator abstraction.
 *
 * Responsibilities:
 *   - Validate constructor dependencies.
 *   - Expose standard service-lifetime values.
 *   - Register service implementations and report owned registration failures.
 *
 * Collaborators:
 *   - IServiceConfigurator
 *   - IServiceCollection
 *   - ILogger<IServiceConfigurator>
 *
 * Copyright:
 *   © 2026 Concept Tech Inc. (CTI). All rights reserved.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CTI.AIDev.Common
{
    internal class ServiceConfigurator : IServiceConfigurator
    {
        private readonly ILogger<IServiceConfigurator> logger;
        private readonly IServiceCollection services;

        public ServiceConfigurator(IServiceCollection services, ILogger<IServiceConfigurator> logger)
        {
            this.logger = logger.NotNull(nameof(logger));
            this.services = services.NotNull(this.logger, nameof(services));
        }

        public ServiceLifetime Transient => ServiceLifetime.Transient;

        public ServiceLifetime Scoped => ServiceLifetime.Scoped;

        public ServiceLifetime Singleton => ServiceLifetime.Singleton;

        public bool HasService<TService>()
            where TService : class
        {
            return services.Any(descriptor => descriptor.ServiceType == typeof(TService));
        }

        public bool ConfigureService<TService, TImplementation>(ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            try
            {
                var descriptor = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
                services.TryAdd(descriptor);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error configuring service {ServiceType} with implementation {ImplementationType}", typeof(TService), typeof(TImplementation));
                return false;
            }
        }
    }
}
