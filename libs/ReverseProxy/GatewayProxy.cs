using System;
using System.Collections.Generic;

namespace CodeM.FastApi.ReverseProxy
{
    public class GatewayProxy
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public string MathPath { get; set; }

        /// <summary>
        /// 0:RemovePrefix， 1:Prefix， 2:Set， 3:Pattern
        /// </summary>
        public int TransformMode { get; set; }

        public string TransformPath { get; set; }

        public TimeSpan? RequestTimeout { get; set; }

        /// <summary>
        /// 负载均衡策略（0：First，1：Random，2：RoundRobin，3：LeastRequests，4：PowerOfTwoChoices）
        /// </summary>
        public int? LoadBalancing { get; set; }

        public bool HealthCheck { get; set; } = false;

        public string HealthCheckPath { get; set; } = null;

        public TimeSpan? HealthCheckInterval { get; set; }

        public TimeSpan? HealthCheckTimeout { get; set; }

        public List<GatewayProxyNode> Nodes { get; } = new List<GatewayProxyNode>();
    }
}
