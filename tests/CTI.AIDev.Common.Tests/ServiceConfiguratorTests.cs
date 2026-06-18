using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CTI.AIDev.Common
{
    public sealed class ServiceConfiguratorTests
    {
        [Fact]
        public void Constructor_NullServices_ThrowsArgumentNullException()
        {
            var logger = NullLogger<IServiceConfigurator>.Instance;

            var act = () => new ServiceConfigurator(null!, logger);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("services");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            var services = new ServiceCollection();

            var act = () => new ServiceConfigurator(services, null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Fact]
        public void LifetimeProperties_ReturnExpectedServiceLifetimes()
        {
            var configurator = new ServiceConfigurator(new ServiceCollection(), NullLogger<IServiceConfigurator>.Instance);

            configurator.Transient.Should().Be(ServiceLifetime.Transient);
            configurator.Scoped.Should().Be(ServiceLifetime.Scoped);
            configurator.Singleton.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void ConfigureService_NewService_ReturnsTrueAndAddsDescriptor()
        {
            var services = new ServiceCollection();
            var configurator = new ServiceConfigurator(services, NullLogger<IServiceConfigurator>.Instance);

            var result = configurator.ConfigureService<ITestService, TestService>(ServiceLifetime.Singleton);

            result.Should().BeTrue();
            services.Should().ContainSingle(descriptor =>
                descriptor.ServiceType == typeof(ITestService)
                && descriptor.ImplementationType == typeof(TestService)
                && descriptor.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void ConfigureService_DuplicateService_ReturnsTrueWithoutAddingSecondDescriptor()
        {
            var services = new ServiceCollection();
            var configurator = new ServiceConfigurator(services, NullLogger<IServiceConfigurator>.Instance);

            var firstResult = configurator.ConfigureService<ITestService, TestService>(ServiceLifetime.Singleton);
            var secondResult = configurator.ConfigureService<ITestService, AlternateTestService>(ServiceLifetime.Transient);

            firstResult.Should().BeTrue();
            secondResult.Should().BeTrue();
            services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(ITestService));
            services.Single(descriptor => descriptor.ServiceType == typeof(ITestService)).ImplementationType
                .Should().Be(typeof(TestService));
        }

        [Fact]
        public void ConfigureService_AddFailure_ReturnsFalseAndLogsError()
        {
            var logger = new RecordingLogger<IServiceConfigurator>();
            var services = new ThrowingServiceCollection();
            var configurator = new ServiceConfigurator(services, logger);

            var result = configurator.ConfigureService<ITestService, TestService>(ServiceLifetime.Scoped);

            result.Should().BeFalse();
            logger.Entries.Should().ContainSingle();
            logger.Entries[0].LogLevel.Should().Be(LogLevel.Error);
            logger.Entries[0].Exception.Should().BeOfType<InvalidOperationException>();
            logger.Entries[0].Message.Should().Contain("Error configuring service");
        }

        private interface ITestService;

        private sealed class TestService : ITestService;

        private sealed class AlternateTestService : ITestService;

        private sealed class ThrowingServiceCollection : IServiceCollection
        {
            private readonly List<ServiceDescriptor> descriptors = new();

            public ServiceDescriptor this[int index]
            {
                get => descriptors[index];
                set => descriptors[index] = value;
            }

            public int Count => descriptors.Count;

            public bool IsReadOnly => false;

            public void Add(ServiceDescriptor item)
            {
                throw new InvalidOperationException("Add failed");
            }

            public void Clear()
            {
                descriptors.Clear();
            }

            public bool Contains(ServiceDescriptor item)
            {
                return descriptors.Contains(item);
            }

            public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
            {
                descriptors.CopyTo(array, arrayIndex);
            }

            public IEnumerator<ServiceDescriptor> GetEnumerator()
            {
                return descriptors.GetEnumerator();
            }

            public int IndexOf(ServiceDescriptor item)
            {
                return descriptors.IndexOf(item);
            }

            public void Insert(int index, ServiceDescriptor item)
            {
                descriptors.Insert(index, item);
            }

            public bool Remove(ServiceDescriptor item)
            {
                return descriptors.Remove(item);
            }

            public void RemoveAt(int index)
            {
                descriptors.RemoveAt(index);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return descriptors.GetEnumerator();
            }
        }

        private sealed class RecordingLogger<TCategory> : ILogger<TCategory>
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
            }
        }

        private sealed record LogEntry(LogLevel LogLevel, string Message, Exception? Exception);
    }
}