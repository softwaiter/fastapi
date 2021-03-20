using CodeM.FastApi.Context;
using CodeM.FastApi.Logger;
using CodeM.FastApi.Services;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class ResetPass
    {
        public async Task Handle(ControllerContext cc)
        {
            string userId = cc.RouteParams["userid"];
            dynamic user = UserService.GetUserById(userId);
            if (user == null)
            {
                await cc.JsonAsync(-1, null, "用户不存在。");
                return;
            }
            if (!user.Actived)
            {
                await cc.JsonAsync(-1, null, "用户已禁用。");
                return;
            }
            if (user.Deleted)
            {
                await cc.JsonAsync(-1, null, "用户已冻结。");
                return;
            }

            dynamic org = OrgService.GetOrgByCode(user.Org);
            if (!org.Actived)
            {
                await cc.JsonAsync(-1, null, "用户所属机构已禁用。");
                return;
            }
            if (org.Deleted)
            {
                await cc.JsonAsync(-1, null, "用户所属机构已冻结。");
                return;
            }

            string pass = cc.PostJson != null ? cc.PostJson.p : null;
            string pass2 = cc.PostJson != null ? cc.PostJson.p2 : null;
            if (string.IsNullOrWhiteSpace(pass))
            {
                await cc.JsonAsync(-1, null, "密码不能为空。");
                return;
            }
            if (pass != pass2)
            {
                await cc.JsonAsync(-1, null, "两次输入密码不一致。");
                return;
            }

            try
            {
                UserService.UpdatePassword(userId, pass);
                await cc.JsonAsync("成功！");
            }
            catch (Exception exp)
            {
                LogUtils.Error(exp);
                await cc.JsonAsync("更新密码失败。");
            }
        }
    }
}
