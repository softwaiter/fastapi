using CodeM.FastApi.Context;
using CodeM.FastApi.System.Utils;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class Logout
    {
        public async Task Handle(ControllerContext cc)
        {
            string userCode = LoginUtils.GetLoginUserCode(cc);
            if (!string.IsNullOrWhiteSpace(userCode))
            {
                LoginUtils.SetLoginUserCode(cc, string.Empty);
            }
            await cc.JsonAsync("注销登录成功。");
        }
    }
}
