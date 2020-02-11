using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DispatchProxyDecoratorsWithDependencyInjection.Extensions
{
    internal static class ServiceCollectionsExtensions
    {
        /// <summary>
        /// Method searches <see cref="IServiceCollection"/> for all registered services implementing <see cref="TInterface"/>,
        /// and swaps them with <see cref="TProxy"/> based decorator. Note that created instance will inherit from
        /// <see cref="TProxy" /> (which inherits from <see cref="DispatchProxy"/>) AND implements supplied <see cref="TInterface"/>.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <typeparam name="TInterface">Service type, which you want to decorate.</typeparam>
        /// <typeparam name="TProxy">DispatchProxy child, a decorator.</typeparam>
        /// <returns>Modified service collection</returns>
        /// <exception cref="InvalidOperationException"><see cref="TProxy"/> needs to have a static method
        /// with output type of <see cref="TInterface"/> (usually called "Create")</exception>
        public static IServiceCollection DecorateWithDispatchProxy<TInterface, TProxy>(this IServiceCollection services)
            where TInterface : class
            where TProxy : DispatchProxy
        {
            MethodInfo createMethod;
            try
            {
                createMethod = typeof(TProxy)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(info => !info.IsGenericMethod && info.ReturnType == typeof(TInterface));
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"Looks like there is no static method in {typeof(TProxy)} " +
                                                    $"which creates instance of {typeof(TInterface)} (note that this method should not be generic)", e);
            }

            var argInfos = createMethod.GetParameters();

            // Save all descriptors that needs to be decorated into a list.
            var descriptorsToDecorate = services
                .Where(s => s.ServiceType == typeof(TInterface))
                .ToList();

            if (descriptorsToDecorate.Count == 0)
            {
                throw new InvalidOperationException($"Attempted to Decorate services of type {typeof(TInterface)}, " +
                                                    "but no such services are present in ServiceCollection");
            }

            foreach (var descriptor in descriptorsToDecorate)
            {
                var decorated = ServiceDescriptor.Describe(
                    typeof(TInterface),
                    sp =>
                    {
                        var decoratorInstance = createMethod.Invoke(null,
                            argInfos.Select(
                                    info => info.ParameterType == (descriptor.ServiceType ?? descriptor.ImplementationType)
                                        ? sp.CreateInstance(descriptor)
                                        : sp.GetRequiredService(info.ParameterType))
                                .ToArray());

                        return (TInterface) decoratorInstance;
                    },
                    descriptor.Lifetime);

                services.Remove(descriptor);
                services.Add(decorated);
            }

            return services;
        }

        /// <summary>
        /// Method allows to override previously added service with decorated version.
        /// In case multiple implementations of TInterface were registered, everyone will be decorated.
        /// <para>Code based on https://greatrexpectations.com/2018/10/25/decorators-in-net-core-with-dependency-injection </para>
        /// </summary>
        /// <param name="services">Instance of IServiceCollection.</param>
        /// <typeparam name="TInterface">Service which should be decorated and overriden in ServiceCollection.</typeparam>
        /// <typeparam name="TDecorator">Decorator to use.</typeparam>
        /// <exception cref="InvalidOperationException">In case there is no registered service of type TInterface.</exception>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection DecorateWithClassicDecorator<TInterface, TDecorator>(this IServiceCollection services)
            where TInterface : class
            where TDecorator : class, TInterface
        {
            var objectFactory = ActivatorUtilities.CreateFactory(
                typeof(TDecorator),
                new[] { typeof(TInterface) });

            // Save all descriptors that needs to be decorated into a list.
            var descriptorsToDecorate = services
                .Where(s => s.ServiceType == typeof(TInterface))
                .ToList();

            if (descriptorsToDecorate.Count == 0)
            {
                throw new InvalidOperationException($"Attempted to Decorate services of type {typeof(TInterface)}, " +
                                                    "but no such services are present in ServiceCollection");
            }

            foreach (var descriptor in descriptorsToDecorate)
            {
                // Create new descriptor with prepared object factory.
                var decorated = ServiceDescriptor.Describe(
                    typeof(TInterface),
                    s => (TInterface)objectFactory(s, new[] { s.CreateInstance(descriptor) }),
                    descriptor.Lifetime);

                services.Remove(descriptor);
                services.Add(decorated);
            }

            return services;
        }

        private static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(services);
            }

            return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType);
        }

    }
}