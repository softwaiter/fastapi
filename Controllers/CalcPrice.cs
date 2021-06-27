using CodeM.FastApi.Context;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class CalcPrice
    {

        public async Task Handle(ControllerContext cc)
        {
            string prod = cc.QueryParams.Get("prod", null);
            string month = cc.QueryParams.Get("month", null);

            if (string.IsNullOrWhiteSpace(prod) ||
                string.IsNullOrWhiteSpace(month))
            {
                await cc.JsonAsync(-1, null, "缺少参数。");
                return;
            }

            int monthNum;
            if (!int.TryParse(month, out monthNum))
            {
                await cc.JsonAsync(-1, null, "month参数无效。");
                return;
            }
        }

    }
}
