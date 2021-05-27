using CodeM.FastApi.Services;

namespace CodeM.FastApi.System
{
    public class GlobalData
    {
        public GlobalData()
        { 
        }

        public GlobalData(dynamic user, dynamic org)
        {
            this.User = user;
            this.Org = org;
        }

        public dynamic User { get; set; } = null;

        public dynamic Org { get; set; } = null;

        public dynamic GetUserByCode(string code)
        {
            return UserService.GetUserByCode(code);
        }
    }
}
