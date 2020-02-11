using System;

namespace DispatchProxyDecoratorsWithDependencyInjection.Services
{
    internal sealed class ImportantDataProvider : IImportantDataProvider
    {
        /// <inheritdoc />
        public TimeSpan GetTimeToWait() => TimeSpan.FromSeconds(2);
    }
}