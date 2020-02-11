using System.Threading.Tasks;
using DispatchProxyDecoratorsWithDependencyInjection.Extensions;
using DispatchProxyDecoratorsWithDependencyInjection.Services;
using DispatchProxyDecoratorsWithDependencyInjection.Services.Decorators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DispatchProxyDecoratorsWithDependencyInjection
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            await using var sp = GetServiceProviderWithClassicDecorator(serviceCollection);

            var consumer = sp.GetRequiredService<ExampleConsumer>();

            await consumer.Run();
        }

        static ServiceProvider GetServiceProviderWithDispatchProxyDecorator(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddLogging(configure =>
                {
                    configure.AddConsole();
                    configure.SetMinimumLevel(LogLevel.Information);
                })
                .AddSingleton<IImportantDataProvider, ImportantDataProvider>()
                .AddSingleton<IExampleService, ExampleService>()
                .DecorateWithDispatchProxy<IExampleService, DispatchProxyLoggingDecorator<IExampleService>>()
                .AddSingleton<ExampleConsumer>();

            return serviceCollection.BuildServiceProvider();
        }

        static ServiceProvider GetServiceProviderWithClassicDecorator(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddLogging(configure =>
                {
                    configure.AddConsole();
                    configure.SetMinimumLevel(LogLevel.Information);
                })
                .AddSingleton<IImportantDataProvider, ImportantDataProvider>()
                .AddSingleton<IExampleService, ExampleService>()
                .DecorateWithClassicDecorator<IExampleService, ExampleServiceClassicDecorator>()
                .AddSingleton<ExampleConsumer>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}