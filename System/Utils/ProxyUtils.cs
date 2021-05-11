using CodeM.FastApi.ReverseProxy;
using CodeM.FastApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace CodeM.FastApi.System.Utils
{
    public class ProxyUtils
    {
        public static void Load(IServiceCollection services)
        {
            List<dynamic> proxyList = ProxyService.GetEffectiveListWithoutLaodan();
            foreach (dynamic proxy in proxyList)
            {
                ReverseProxyManager.AddProxy(proxy.Code, proxy.MatchPath, proxy.TransformMode, 
                    proxy.TransformPath, proxy.LoadBalance, TimeSpan.FromSeconds(proxy.RequestTimeout));
            }

            List<dynamic> nodeList = ProxyNodeService.GetEffectiveListWithoutLaodan();
            foreach (dynamic node in nodeList)
            {
                ReverseProxyManager.AddProxyNode(node.Proxy, node.Url, node.Weight);
            }

            ReverseProxyManager.Register(services);
        }

        public static void Reload()
        {
            ReverseProxyManager.Clear();

            List<dynamic> result = ProxyService.GetEffectiveListWithoutLaodan();
            foreach (dynamic proxy in result)
            {
                ReverseProxyManager.AddProxy(proxy.Code, proxy.MatchPath, proxy.TransformMode,
                    proxy.TransformPath, proxy.LoadBalance, TimeSpan.FromSeconds(proxy.RequestTimeout));
            }

            List<dynamic> nodeList = ProxyNodeService.GetEffectiveListWithoutLaodan();
            foreach (dynamic node in nodeList)
            {
                ReverseProxyManager.AddProxyNode(node.Proxy, node.Url, node.Weight);
            }

            ReverseProxyManager.Refresh();
        }

        public static void Run(IApplicationBuilder app)
        {
            ReverseProxyManager.MountRouters(app);
        }
    }
}
