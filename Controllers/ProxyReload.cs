using CodeM.FastApi.Context;
using CodeM.FastApi.System.Utils;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class ProxyReload
    {
        public async Task Handle(ControllerContext cc)
        {
            try
            {
                ProxyUtils.Reload();
                await cc.JsonAsync("代理配置重载成功！");
            }
            catch (Exception exp)
            {
                cc.Error(exp);
                await cc.JsonAsync("代理重新加载失败。");
            }
        }
    }
}
