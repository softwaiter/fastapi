using CodeM.Common.Tools.Security;
using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Utils;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class ChangePass
    {
        public async Task Handle(ControllerContext cc)
        {
            string oldPass = cc.PostJson != null ? cc.PostJson.oldPass : null;
            string newPass = cc.PostJson != null ? cc.PostJson.newPass : null;
            string newPass2 = cc.PostJson != null ? cc.PostJson.newPass2 : null;
            if (string.IsNullOrWhiteSpace(oldPass))
            {
                await cc.JsonAsync(-1, null, "当前密码不能为空。");
                return;
            }
            if (string.IsNullOrWhiteSpace(newPass))
            {
                await cc.JsonAsync(-1, null, "新密码不能为空。");
                return;
            }
            if (newPass != newPass2)
            {
                await cc.JsonAsync(-1, null, "两次输入的新密码不一致。");
                return;
            }

            string userCode = LoginUtils.GetLoginUserCode(cc);

            dynamic userObj = UserService.GetUserByCode(userCode);
            if (userObj == null)
            {
                await cc.JsonAsync(-1, null, "。");
                return;
            }

            string passHashValue = HashUtils.SHA256(oldPass + userObj.Salt);
            if (passHashValue != userObj.Password)
            {
                await cc.JsonAsync(-1, null, "当前密码输入不正确！");
                return;
            }

            try
            {
                UserService.UpdatePassword(userObj.Id + "", newPass);
                await cc.JsonAsync("修改密码成功！");
            }
            catch (Exception exp)
            {
                cc.Error(exp);
                await cc.JsonAsync(-1, null, "修改密码失败。");
            }
        }
    }
}
