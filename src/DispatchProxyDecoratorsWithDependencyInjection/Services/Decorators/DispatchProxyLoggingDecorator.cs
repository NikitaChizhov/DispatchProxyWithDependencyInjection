using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DispatchProxyDecoratorsWithDependencyInjection.Services.Decorators
{
    internal class DispatchProxyLoggingDecorator<TDecorated> : DispatchProxy
    {
        private TDecorated _decorated;

        private ILogger<DispatchProxyLoggingDecorator<TDecorated>> _logger;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                _logger.LogInformation("Logging something before invoking {decoratedClass}.{methodName}",
                    typeof(TDecorated), targetMethod.Name);

                var result = targetMethod.Invoke(_decorated, args);

                if (result is Task resultTask)
                {
                    resultTask.ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            _logger.LogError(task.Exception,
                                "An unhandled exception was raised during execution of {decoratedClass}.{methodName}",
                                typeof(TDecorated), targetMethod.Name);
                        }
                        _logger.LogInformation("Log something after {decoratedClass}.{methodName} completed",
                            typeof(TDecorated), targetMethod.Name);
                    });
                }
                else
                {
                    _logger.LogInformation("Logging something after method {decoratedClass}.{methodName} completion.",
                        typeof(TDecorated), targetMethod.Name);
                }

                return result;
            }
            catch (TargetInvocationException ex)
            {
                _logger.LogError(ex.InnerException ?? ex,
                    "Error during invocation of {decoratedClass}.{methodName}",
                    typeof(TDecorated), targetMethod.Name);
                throw ex.InnerException ?? ex;
            }
        }

        /// <summary>
        /// Creates and instantiates new class which inherits from <see cref="DispatchProxyLoggingDecorator{TDecorated}"/>
        /// and implements <see cref="TDecorated"/>
        /// </summary>
        /// <param name="decorated"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static TDecorated Create(TDecorated decorated, ILogger<DispatchProxyLoggingDecorator<TDecorated>> logger)
        {
            object proxy = Create<TDecorated, DispatchProxyLoggingDecorator<TDecorated>>();
            ((DispatchProxyLoggingDecorator<TDecorated>)proxy).SetParameters(decorated, logger);

            return (TDecorated)proxy;
        }

        private void SetParameters(TDecorated decorated, ILogger<DispatchProxyLoggingDecorator<TDecorated>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        }
    }
}