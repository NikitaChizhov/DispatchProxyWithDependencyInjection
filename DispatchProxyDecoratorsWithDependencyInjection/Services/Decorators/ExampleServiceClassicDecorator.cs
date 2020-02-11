using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DispatchProxyDecoratorsWithDependencyInjection.Services.Decorators
{
    internal sealed class ExampleServiceClassicDecorator : IExampleService
    {
        private readonly IExampleService _decorated;

        private readonly ILogger<ExampleServiceClassicDecorator> _logger;

        public ExampleServiceClassicDecorator(IExampleService decorated, ILogger<ExampleServiceClassicDecorator> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task VeryImportantWorkAsync(int additionalSecondsToWait)
        {
            _logger.LogInformation("Log something before method call");
            await _decorated.VeryImportantWorkAsync(additionalSecondsToWait);
            _logger.LogInformation("Log something after method competition.");
        }
    }
}