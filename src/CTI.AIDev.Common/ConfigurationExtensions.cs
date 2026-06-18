/*
 * Purpose:
 *   Exposes the supporting-library entry point for creating service configurators.
 *
 * Responsibilities:
 *   - Validate configuration-extension arguments.
 *   - Create an IServiceConfigurator for the supplied service collection.
 *   - Keep configurator creation on the common abstraction surface.
 *
 * Collaborators:
 *   - IServiceCollection
 *   - IServiceConfigurator
 *   - ServiceConfigurator
 *
 * Copyright:
 *   © 2026 Concept Tech Inc. (CTI). All rights reserved.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CTI.AIDev.Common
{ 
    internal static class ConfigurationExtensions
    {
        public static IServiceConfigurator GetConfigurator(this IServiceCollection services, ILogger<IServiceConfigurator> logger)
        {
            var validatedLogger = logger.NotNull(nameof(logger));
            IServiceCollection validatedServices = services.NotNull(validatedLogger, nameof(services));

            return new ServiceConfigurator(validatedServices, validatedLogger);
        }
    }
}
