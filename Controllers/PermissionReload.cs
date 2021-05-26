using CodeM.FastApi.Context;
using CodeM.FastApi.System.Utils;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class PermissionReload
    {
        public async Task Handle(ControllerContext cc)
        {
            try
            {
                PermissionUtils.Reload();
                await cc.JsonAsync("权限设置重载成功！");
            }
            catch (Exception exp)
            {
                cc.Error(exp);
                await cc.JsonAsync(-1, null, "权限设置重新加载失败。");
            }
        }
    }
}
