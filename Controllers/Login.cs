using CodeM.Common.Orm;
using CodeM.Common.Tools.Security;
using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class Login
    {
        public async Task Handle(ControllerContext cc)
        {
            if (cc.PostJson == null)
            {
                await cc.JsonAsync(-1, null, "无效的参数。");
                return;
            }

            string user = cc.PostJson.u;
            string pass = cc.PostJson.p;
            string from = cc.PostJson.f;

            dynamic userObj = OrmUtils.Model("User").Equals("Code", user)
                .Or(new SubFilter().Equals("Mobile", user))
                .Or(new SubFilter().Equals("Email", user))
                .QueryFirst();
            if (userObj == null)
            {
                await cc.JsonAsync(-1, null, "用户名或密码错误。");
                return;
            }
            if (!userObj.Actived)
            {
                await cc.JsonAsync(-1, null, "用户已禁用。");
                return;
            }
            if (userObj.Deleted)
            {
                await cc.JsonAsync(-1, null, "用户已冻结。");
                return;
            }
            if (userObj.Expires < DateTime.Now)
            {
                await cc.JsonAsync(-1, null, "用户使用期限已到期。");
                return;
            }

            dynamic orgObj = OrgService.GetOrgByCode(userObj.Org);
            if (orgObj == null)
            {
                await cc.JsonAsync(-1, null, "用户所属机构不存在。");
                return;
            }
            if (!orgObj.Actived)
            {
                await cc.JsonAsync(-1, null, "用户所属机构已禁用。");
                return;
            }
            if (orgObj.Deleted)
            {
                await cc.JsonAsync(-1, null, "用户所属机构已冻结。");
                return;
            }

            string passHashValue = HashUtils.SHA256(pass + userObj.Salt);
            if (passHashValue != userObj.Password)
            {
                await cc.JsonAsync(-1, null, "用户名或密码错误！");
                return;
            }

            await cc.JsonAsync("Hello");
        }
    }
}
