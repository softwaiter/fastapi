using CodeM.FastApi.Config;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CodeM.FastApi.System.Middlewares
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private ApplicationConfig mConfig;

        public PermissionMiddleware(RequestDelegate next, ApplicationConfig config)
        {
            _next = next;
            mConfig = config;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
        }
    }
}