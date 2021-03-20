using CodeM.FastApi.ReverseProxy;
using Microsoft.ReverseProxy.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InMemoryConfigProviderExtensions
    {
        public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder, InMemoryConfigProvider config)
        {
            builder.Services.AddSingleton<IProxyConfigProvider>(config);
            return builder;
        }
    }
}
