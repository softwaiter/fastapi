namespace CodeM.FastApi.ReverseProxy
{
    public class GatewayProxyNode
    {
        public string Address { get; set; }

        public int Weight { get; set; }
    }
}
