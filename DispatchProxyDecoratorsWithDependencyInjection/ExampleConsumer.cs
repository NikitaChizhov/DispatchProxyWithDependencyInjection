using System.Threading.Tasks;
using DispatchProxyDecoratorsWithDependencyInjection.Services;

namespace DispatchProxyDecoratorsWithDependencyInjection
{
    internal sealed class ExampleConsumer
    {
        private readonly IExampleService _exampleService;

        public ExampleConsumer(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }

        public async Task Run() => await _exampleService.VeryImportantWorkAsync(1);
    }
}