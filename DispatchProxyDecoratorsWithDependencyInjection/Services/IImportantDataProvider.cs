using System;

namespace DispatchProxyDecoratorsWithDependencyInjection.Services
{
    internal interface IImportantDataProvider
    {
        TimeSpan GetTimeToWait();
    }
}