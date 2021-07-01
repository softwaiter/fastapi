using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Utils;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class OpenProductForOrg
    {
        public async Task Handle(ControllerContext cc)
        {
            if (cc.PostJson == null)
            {
                await cc.JsonAsync(-1, null, "缺少参数。");
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
                dynamic prod = ProductService.GetProductByCode(prodCode);
                if (prod == null)
                {
                    await cc.JsonAsync(-1, null, "不识别的产品服务。");
                    return;
                }

                string userCode = LoginUtils.GetLoginUserCode(cc);
                dynamic user = UserService.GetUserByCode(userCode);

                dynamic orgProd = OrgProdService.GetOrgProdByCode(user.Org, prodCode);
                if (orgProd != null)
                {
                    await cc.JsonAsync(-1, null, "指定产品已开通，无需重复申请。");
                    return;
                }

                OpenProductForOrgService.Open(user.Org, userCode, prod);

                await cc.JsonAsync("产品开通成功！");
            }
            catch (Exception exp)
            {
                cc.Error(exp);
                await cc.JsonAsync(-1, null, "产品开通失败。");
            }
        }
    }
}