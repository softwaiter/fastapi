﻿using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading;
using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.Service;

namespace CodeM.FastApi.ReverseProxy
{
    public class InMemoryConfigProvider : IProxyConfigProvider
    {
        private volatile InMemoryConfig _config;

        public InMemoryConfigProvider(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            _config = new InMemoryConfig(routes, clusters);
        }

        public IProxyConfig GetConfig() => _config;

        public void Update(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            var oldConfig = _config;
            _config = new InMemoryConfig(routes, clusters);
            oldConfig.SignalChange();
        }

        private class InMemoryConfig : IProxyConfig
        {
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            public InMemoryConfig(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
            {
                Routes = routes;
                Clusters = clusters;
                ChangeToken = new CancellationChangeToken(_cts.Token);
            }

            public IReadOnlyList<ProxyRoute> Routes { get; }

            public IReadOnlyList<Cluster> Clusters { get; }

            public IChangeToken ChangeToken { get; }

            internal void SignalChange()
            {
                _cts.Cancel();
            }
        }
    }
}
