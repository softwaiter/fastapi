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
            string from = cc.Headers.Get("platform", null);
            if (cc.PostJson == null || string.IsNullOrWhiteSpace(from))
            {
                await cc.JsonAsync(-1, null, "缺少参数。");
                return;
            }

            string user = cc.PostJson.u;
            string pass = cc.PostJson.p;
            if (user == null || pass == null)
            {
                await cc.JsonAsync(-1, null, "无效的参数。");
                return;
            }

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

            dynamic prodObj = ProductService.GetProductByCode(from);
            if (prodObj == null)
            {
                await cc.JsonAsync(-1, null, "无效的业务平台。");
                return;
            }
            if (!prodObj.Actived)
            {
                await cc.JsonAsync(-1, null, "业务平台已禁用。");
                return;
            }
            if (prodObj.Deleted)
            {
                await cc.JsonAsync(-1, null, "业务平台已冻结。");
                return;
            }

            dynamic orgprodObj = OrgProdService.GetOrgProdByCode(userObj.Org, from);
            if (orgprodObj == null)
            {
                await cc.JsonAsync(-1, null, "缺少业务平台权限。");
                return;
            }
            if (!orgprodObj.Actived)
            {
                await cc.JsonAsync(-1, null, "业务平台权限已禁用。");
                return;
            }
            if (orgprodObj.Deleted)
            {
                await cc.JsonAsync(-1, null, "业务平台权限已冻结。");
                return;
            }
            if (orgprodObj.Expires < DateTime.Now)
            {
                await cc.JsonAsync(-1, null, "业务平台权限已到期。");
                return;
            }

            dynamic userprodObj = UserProdService.GetUserProdByCode(userObj.Code, from);
            if (userprodObj == null)
            {
                await cc.JsonAsync(-1, null, "缺少业务平台权限！");
                return;
            }
            if (!userprodObj.Actived)
            {
                await cc.JsonAsync(-1, null, "业务平台权限已禁用！");
                return;
            }
            if (userprodObj.Deleted)
            {
                await cc.JsonAsync(-1, null, "业务平台权限已冻结！");
                return;
            }
            if (userprodObj.Expires < DateTime.Now)
            {
                await cc.JsonAsync(-1, null, "业务平台权限已到期！");
                return;
            }

            //todo 生成token并存储，对应user id或code
            //toto 获取用户的时候，同时存储缓存，使用user id或code做key

            cc.Session.SetString("code", userObj.Code);
            
            await cc.JsonAsync(cc.Session.Id);
        }
    }
}
