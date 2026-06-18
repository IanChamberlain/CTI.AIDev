/*
 * Purpose:
 *   Verifies the CTI.AIDev.Common configuration extension surface in the supporting-library layer.
 *
 * Responsibilities:
 *   - Validate extension-method guard behavior.
 *   - Validate configurator creation through the public extension surface.
 *   - Validate that returned configurators operate on the supplied service collection.
 *
 * Collaborators:
 *   - ConfigurationExtensions
 *   - IServiceConfigurator
 *   - ServiceConfigurator
 *
 * Copyright:
 *   © 2026 Concept Tech Inc. (CTI). All rights reserved.
 */

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CTI.AIDev.Common;

public sealed class ConfigurationExtensionsTests
{
    [Fact]
    public void GetConfigurator_NullServices_ThrowsArgumentNullException()
    {
        var logger = NullLogger<IServiceConfigurator>.Instance;

        var act = () => ConfigurationExtensions.GetConfigurator(null!, logger);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void GetConfigurator_NullLogger_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        var act = () => services.GetConfigurator(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void GetConfigurator_ValidArguments_ReturnsServiceConfigurator()
    {
        var services = new ServiceCollection();
        var logger = NullLogger<IServiceConfigurator>.Instance;

        var configurator = services.GetConfigurator(logger);

        configurator.Should().BeOfType<ServiceConfigurator>();
    }

    [Fact]
    public void GetConfigurator_ReturnedConfigurator_RegistersServicesIntoSuppliedCollection()
    {
        var services = new ServiceCollection();
        var logger = NullLogger<IServiceConfigurator>.Instance;
        var configurator = services.GetConfigurator(logger);

        var result = configurator.ConfigureService<ITestService, TestService>(ServiceLifetime.Transient);

        result.Should().BeTrue();
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(ITestService)
            && descriptor.ImplementationType == typeof(TestService)
            && descriptor.Lifetime == ServiceLifetime.Transient);
    }

    private interface ITestService;

    private sealed class TestService : ITestService;
}