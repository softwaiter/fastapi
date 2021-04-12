using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.Abstractions.ClusterDiscovery.Contract;
using Yarp.ReverseProxy.Abstractions.Config;
using Yarp.ReverseProxy.Service.Proxy;

namespace CodeM.FastApi.ReverseProxy
{
    public class ReverseProxyManager
    {
        private static ConcurrentDictionary<string, GatewayProxy> sItems = new ConcurrentDictionary<string, GatewayProxy>();
        private static InMemoryConfigProvider sProxyConfig = null;

        public static bool AddProxy(string code, string matchPath, int transMode, string transPath, int? loadBalancing, TimeSpan? requestTimeout, 
            bool healthCheck, string healthCheckPath, TimeSpan? healthCheckInterval, TimeSpan? healthCheckTimeout)
        {
            GatewayProxy prxoy = new GatewayProxy();
            prxoy.Code = code;
            prxoy.MathPath = matchPath;
            prxoy.TransformMode = transMode;
            prxoy.TransformPath = transPath;
            prxoy.LoadBalancing = loadBalancing ?? null;
            prxoy.RequestTimeout = requestTimeout;
            prxoy.HealthCheck = healthCheck;
            prxoy.HealthCheckPath = healthCheckPath;
            prxoy.HealthCheckInterval = healthCheckInterval;
            prxoy.HealthCheckTimeout = healthCheckTimeout;
            return sItems.TryAdd(code.ToLower(), prxoy);
        }

        public static bool AddProxy(string code, string matchPath, int transMode, string transPath, int? loadBalancing, TimeSpan? requestTimeout)
        {
            return AddProxy(code, matchPath, transMode, transPath, loadBalancing, requestTimeout, false, null, null, null);
        }

        public static bool AddProxy(string code, string matchPath, int transMode, string transPath)
        {
            return AddProxy(code, matchPath, transMode, transPath, null, null);
        }

        public static bool AddProxyNode(string proxyCode, string address, int weight)
        {
            GatewayProxy proxy;
            if (sItems.TryGetValue(proxyCode.ToLower(), out proxy))
            {
                GatewayProxyNode node = new GatewayProxyNode();
                node.Address = address;
                node.Weight = weight;
                proxy.Nodes.Add(node);
                return true;
            }
            return false;
        }

        public static void Clear()
        {
            sItems.Clear();
        }

        private static List<ProxyRoute> GenerateRoutes()
        {
            List<ProxyRoute> result = new List<ProxyRoute>();

            IEnumerator<KeyValuePair<string, GatewayProxy>> e = sItems.GetEnumerator();
            while (e.MoveNext())
            {
                GatewayProxy gp = e.Current.Value;

                ProxyRoute pr = new ProxyRoute
                {
                    RouteId = string.Concat(gp.Code, "_router"),
                    ClusterId = string.Concat(gp.Code, "_cluster"),
                    Match = new ProxyMatch() 
                    {
                        Path = gp.MathPath
                    }
                };

                switch (gp.TransformMode)
                {
                    case 0: //RemovePrefix
                        pr = pr.WithTransformPathRemovePrefix(prefix: gp.TransformPath);
                        break;
                    case 1: //Prefix
                        pr = pr.WithTransformPathPrefix(prefix: gp.TransformPath);
                        break;
                    case 2: //Set
                        pr = pr.WithTransformPathSet(path: gp.TransformPath);
                        break;
                    case 3: //Pattern
                        pr = pr.WithTransformPathRouteValues(pattern: new PathString(gp.TransformPath));
                        break;
                }

                result.Add(pr);
            }

            return result;
        }

        public static int GetGCD(List<int> listOri)
        {
            List<int> list = new List<int>(listOri);

            int c = 1;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i - 1] < list[i]) // 确定a>b
                {
                    list[i - 1] = list[i - 1] + list[i];
                    list[i] = list[i - 1] - list[i];
                    list[i - 1] = list[i - 1] - list[i];
                }
                for (c = list[i]; c >= 1; c--)
                {
                    if (list[i - 1] % c == 0 && list[i] % c == 0)
                        break;
                }
                list[i] = c;
            }
            return c;
        }

        private static List<Cluster> GenerateClusters()
        {
            List<Cluster> result = new List<Cluster>();

            IEnumerator<KeyValuePair<string, GatewayProxy>> e = sItems.GetEnumerator();
            while (e.MoveNext())
            {
                GatewayProxy proxy = e.Current.Value;

                int gcd = 1;
                if (proxy.Nodes.Count == 1)
                {
                    GatewayProxyNode node = proxy.Nodes[0];
                    gcd = node.Weight;
                }
                else if (proxy.Nodes.Count > 1)
                {
                    List<int> _weightNodes = new List<int>();
                    for (int i = 0; i < proxy.Nodes.Count; i++)
                    {
                        GatewayProxyNode node = proxy.Nodes[i];
                        _weightNodes.Add(node.Weight);
                    }
                    gcd = GetGCD(_weightNodes);
                }

                Dictionary<string, Destination> _nodeList = new Dictionary<string, Destination>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < proxy.Nodes.Count; i++)
                {
                    GatewayProxyNode node = proxy.Nodes[i];
                    for (int j = 0; j < node.Weight / gcd; j++)
                    {
                        Destination dest = new Destination
                        {
                            Address = node.Address
                        };
                        _nodeList.Add(string.Concat(proxy.Code, "_dest_", i + 1, "_", j + 1), dest);
                    }
                }

                string loadBalancing = null;
                if (_nodeList.Count > 0 && proxy.LoadBalancing != null)
                {
                    switch (proxy.LoadBalancing.Value)
                    {
                        case 0:
                            loadBalancing = LoadBalancingPolicies.First;
                            break;
                        case 1:
                            loadBalancing = LoadBalancingPolicies.Random;
                            break;
                        case 2:
                            loadBalancing = LoadBalancingPolicies.RoundRobin;
                            break;
                        case 3:
                            loadBalancing = LoadBalancingPolicies.LeastRequests;
                            break;
                        case 4:
                            loadBalancing = LoadBalancingPolicies.PowerOfTwoChoices;
                            break;
                    }
                }

                HealthCheckOptions healthCheckOptions = null;
                if (proxy.HealthCheck && !string.IsNullOrWhiteSpace(proxy.HealthCheckPath))
                {
                    healthCheckOptions = new HealthCheckOptions
                    {
                        Active = new ActiveHealthCheckOptions
                        {
                            Enabled = proxy.HealthCheck,
                            Path = proxy.HealthCheckPath,
                            Interval = proxy.HealthCheckInterval,
                            Timeout = proxy.HealthCheckTimeout
                        },
                        Passive = null
                    };
                }

                RequestProxyOptions requestProxyOptions = null;
                if (proxy.RequestTimeout != null)
                {
                    requestProxyOptions = new RequestProxyOptions
                    {
                        Timeout = proxy.RequestTimeout
                    };
                }

                Cluster c = new Cluster
                {
                    Id = string.Concat(proxy.Code, "_cluster"),
                    Destinations = _nodeList,
                    LoadBalancingPolicy = loadBalancing,
                    HealthCheck = healthCheckOptions,
                    HttpRequest = requestProxyOptions
                };

                result.Add(c);
            }

            return result;
        }

        public static void Register(IServiceCollection services)
        {
            List<ProxyRoute> prs = GenerateRoutes();
            List<Cluster> cs = GenerateClusters();
            sProxyConfig = new InMemoryConfigProvider(prs, cs);
            services.AddReverseProxy().LoadFromMemory(sProxyConfig);
        }

        public static void MountRouters(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
            });
        }

        public static void Refresh()
        {
            if (sProxyConfig != null)
            {
                List<ProxyRoute> prs = GenerateRoutes();
                List<Cluster> cs = GenerateClusters();
                sProxyConfig.Update(prs, cs);
            }
            else
            {
                throw new Exception("Register First.");
            }
        }

    }
}
