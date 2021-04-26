using CodeM.Common.Orm;
using CodeM.Common.Tools.Security;
using CodeM.FastApi.Context;
using CodeM.FastApi.System.Utils;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class Login
    {
        public async Task Handle(ControllerContext cc)
        {
            if (cc.PostJson == null)
            {
                await cc.JsonAsync(-1, null, "缺少参数。");
                return;
            }

            string user = cc.PostJson.u;
            string pass = cc.PostJson.p;
            if (string.IsNullOrWhiteSpace(user) || 
                string.IsNullOrWhiteSpace(pass))
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

            string error;
            string platform = cc.Headers.Get("Platform", null);
            if (!LoginUtils.CheckUserValidity(userObj, platform, out error))
            {
                await cc.JsonAsync(-1, null, error);
                return;
            }

            string passHashValue = HashUtils.SHA256(pass + userObj.Salt);
            if (passHashValue != userObj.Password)
            {
                await cc.JsonAsync(-1, null, "用户名或密码错误！");
                return;
            }

            //todo 生成token并存储，对应user id或code
            //toto 获取用户的时候，同时存储缓存，使用user id或code做key

            LoginUtils.SetLoginUserCode(cc, userObj.Code);

            await cc.JsonAsync(cc.Session.Id);
        }
    }
}
