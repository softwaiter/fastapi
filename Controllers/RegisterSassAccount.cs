using CodeM.Common.Tools;
using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Utils;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class RegisterSassAccount
    {
        public async Task Handle(ControllerContext cc)
        {
            if (cc.PostJson == null)
            {
                await cc.JsonAsync(-1, null, "缺少参数。");
                return;
            }

            string mobile = cc.PostJson.mobile;
            string email = cc.PostJson.email;
            string orgName = cc.PostJson.org;
            string name = cc.PostJson.name;
            string pass = cc.PostJson.pass;
            string prod = cc.PostJson.prod;

            if (string.IsNullOrWhiteSpace(mobile) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(orgName) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(pass))
            {
                await cc.JsonAsync(-1, null, "无效的参数。");
                return;
            }

            mobile = mobile.Trim();
            email = email.Trim();
            orgName = orgName.Trim();
            name = name.Trim();
            pass = pass.Trim();

            if (!RegexUtils.IsMobile(mobile))
            {
                await cc.JsonAsync(-1, null, "手机号不合法。");
                return;
            }

            if (!RegexUtils.IsEmail(email))
            {
                await cc.JsonAsync(-1, null, "邮箱地址不合法。");
                return;
            }

            if (orgName.Length < 5)
            {
                await cc.JsonAsync(-1, null, "企业名称不得少于5个字。");
                return;
            }

            dynamic userObj = UserService.GetUserByMobile(mobile);
            if (userObj != null)
            {
                await cc.JsonAsync(-1, null, "该手机号已注册，请直接登录。");
                return;
            }

            userObj = UserService.GetUserByEmail(email);
            if (userObj != null)
            {
                await cc.JsonAsync(-1, null, "该邮箱地址已注册，请直接登录。");
                return;
            }

            dynamic orgObj = OrgService.GetOrgByName(orgName);
            if (orgObj != null)
            {
                await cc.JsonAsync(-1, null, "同名企业已存在，请使用其他企业名称注册或联系管理员处理。");
                return;
            }

            dynamic prodObj = null;
            if (!string.IsNullOrWhiteSpace(prod))
            {
                prodObj = ProductService.GetProductByCode(prod);
                if (prodObj == null)
                {
                    await cc.JsonAsync(-1, null, "不识别的产品标识。");
                    return;
                }
            }

            try
            {
                dynamic newAccount = RegisterSaasAccountService.Register(mobile, email, orgName, name, pass, prodObj);
                LoginUtils.SetLoginUserCode(cc, newAccount.Code);
                await cc.JsonAsync(cc.Session.Id);
            }
            catch (Exception exp)
            {
                cc.Error(exp);
                await cc.JsonAsync(-1, null, "账号注册失败。");
            }
        }
    }
}
