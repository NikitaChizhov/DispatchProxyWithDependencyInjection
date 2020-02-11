using System;
using System.Threading.Tasks;

namespace DispatchProxyDecoratorsWithDependencyInjection.Services
{
    internal sealed class ExampleService : IExampleService
    {
        private readonly IImportantDataProvider _dataProvider;

        public ExampleService(IImportantDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        /// <inheritdoc />
        public async Task VeryImportantWorkAsync(int additionalSecondsToWait)
        {
            var timeToWait = _dataProvider.GetTimeToWait() + TimeSpan.FromSeconds(additionalSecondsToWait);
            await Task.Delay(timeToWait);
        }
    }
}