using System.Threading.Tasks;

namespace DispatchProxyDecoratorsWithDependencyInjection.Services
{
    internal interface IExampleService
    {
        Task VeryImportantWorkAsync(int additionalSecondsToWait);
    }
}