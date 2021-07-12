using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class OpenApi
    {
        public async Task Query(ControllerContext cc)
        {
            string userid = cc.RouteParams["userid"];
            if (string.IsNullOrWhiteSpace(userid))
            {
                await cc.JsonAsync(-1, null, "非法的路由参数。");
                return;
            }

            dynamic userKey = UserKeyService.GetOpenApiByUserId(userid);
            if (userKey != null)
            {
                await cc.JsonAsync(new
                {
                    OpenId = userKey.OpenId,
                    PublicKey = userKey.PublicKey,
                    Actived = userKey.Actived
                });
            }
            else
            {
                await cc.JsonAsync(null);
            }
        }

        public async Task Create(ControllerContext cc)
        {
            string userid = cc.RouteParams["userid"];
            if (string.IsNullOrWhiteSpace(userid))
            {
                await cc.JsonAsync(-1, null, "非法的路由参数。");
                return;
            }

            dynamic userKey = UserKeyService.GetOpenApiByUserId(userid);
            if (userKey == null)
            {
                userKey = UserKeyService.NewOpenApi(userid);
            }

            await cc.JsonAsync(new
            {
                OpenId = userKey.OpenId,
                PublicKey = userKey.PublicKey,
                Actived = userKey.Actived
            });
        }

        public async Task Replace(ControllerContext cc)
        {
            string userid = cc.RouteParams["userid"];
            if (string.IsNullOrWhiteSpace(userid))
            {
                await cc.JsonAsync(-1, null, "非法的路由参数。");
                return;
            }

            dynamic userKey = UserKeyService.ReplaceSecret(userid);
            if (userKey != null)
            {
                await cc.JsonAsync(new
                {
                    OpenId = userKey.OpenId,
                    PublicKey = userKey.PublicKey,
                    Actived = userKey.Actived
                });
            }
            else
            {
                await cc.JsonAsync(-1, null, "请先创建密钥。");
            }
        }

        public async Task Enable(ControllerContext cc)
        {
            string userid = cc.RouteParams["userid"];
            if (string.IsNullOrWhiteSpace(userid))
            {
                await cc.JsonAsync(-1, null, "非法的路由参数。");
                return;
            }

            if (cc.PostJson == null ||
                cc.PostJson.enable == null)
            {
                await cc.JsonAsync(-1, null, "无效的参数。");
                return;
            }

            bool bRet = UserKeyService.Enable(userid, cc.PostJson.enable);
            if (bRet)
            {
                await cc.JsonAsync("SUCC");
            }
            else
            {
                await cc.JsonAsync(-1, null, "请先创建密钥。");
            }
        }
    }
}