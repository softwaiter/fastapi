using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Utils;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class AuthorizeProductForUser
    {
        public async Task Handle(ControllerContext cc)
        {
            if (cc.PostJson == null)
            {
                await cc.JsonAsync(-1, null, "缺少参数。");
                return;
            }

            string userCode = cc.PostJson.user;
            if (string.IsNullOrWhiteSpace(userCode))
            {
                await cc.JsonAsync(-1, null, "无效的参数。");
                return;
            }

            string prodCode = cc.PostJson.prod;
            if (string.IsNullOrWhiteSpace(prodCode))
            {
                await cc.JsonAsync(-1, null, "无效的参数。");
                return;
            }
            prodCode = prodCode.Trim();

            try
            {
                string loginUserCode = LoginUtils.GetLoginUserCode(cc);
                dynamic loginUser = UserService.GetUserByCode(loginUserCode);
                AuthorizeProductForUserService.Auth(loginUser.Org, loginUserCode, userCode, prodCode);
                await cc.JsonAsync("产品授权成功。");
            }
            catch (Exception exp)
            {
                cc.Error(exp);
                await cc.JsonAsync(-1, null, exp.Message);
            }
        }
    }
}